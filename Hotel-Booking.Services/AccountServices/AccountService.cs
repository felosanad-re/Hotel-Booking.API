using AutoMapper;
using Hotel_booking.Core;
using Hotel_booking.Core.Models.Accounts;
using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.RefreshToken;
using Hotel_booking.Core.Options;
using Hotel_booking.Core.Services.Contract.AccountServices;
using Hotel_booking.Core.Services.Contract.TokenServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace Hotel_Booking.Services.AccountServices
{
    public class AccountService : IAccountService
    {
        #region Services
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly ICreateTokenService _createTokenService;
        protected readonly IRefreshTokenService _refreshTokenService;
        protected readonly JwtOptions _jwtOptions;
        protected readonly IMapper _mapper;
        protected readonly IConfiguration _configuration;
        protected readonly IEmailSender _emailSender;
        protected readonly ICurrentUserService _currentUserService;

        public AccountService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICreateTokenService createTokenService,
            IRefreshTokenService refreshTokenService,
            IOptions<JwtOptions> jwtOptions,
            IMapper mapper,
            IConfiguration configuration,
            IEmailSender emailSender,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _createTokenService = createTokenService;
            _refreshTokenService = refreshTokenService;
            _jwtOptions = jwtOptions.Value;
            _mapper = mapper;
            _configuration = configuration;
            _emailSender = emailSender;
            _currentUserService = currentUserService;
        }
        #endregion

        #region Login Async
        public async Task<ApplicationServiceResult<LoginResult>> LoginAsync(LoginRequest req)
        {
            req.EmailOrUserName = req.EmailOrUserName.Trim().ToLower();

            var user = req.EmailOrUserName.Contains('@')
                ? await _userManager.FindByEmailAsync(req.EmailOrUserName)
                : await _userManager.FindByNameAsync(req.EmailOrUserName);

            if (user is null)
                return ApplicationServiceResult<LoginResult>.Fail("User not found");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                req.Password,
                req.RememberMe,
                true
                );

            if (result.IsLockedOut)
                return ApplicationServiceResult<LoginResult>.Fail("Email Is Lockout");
            if (result.IsNotAllowed)
                return ApplicationServiceResult<LoginResult>.Fail("Email Not Confirm");
            if (!result.Succeeded)
                return ApplicationServiceResult<LoginResult>.Fail("Email Or Password Is Incorrect");

            // Create Token
            var token = await _createTokenService.CreateTokenAsync(user);

            // Create Refresh Token
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            // Get user Role
            var roles = await _userManager.GetRolesAsync(user);

            var res = new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                Email = user.Email!,
                Roles = roles,
                UserName = user.UserName!
            };
            return ApplicationServiceResult<LoginResult>.Success(new LoginResult
            {
                RefreshToken = refreshToken,
                Response = res
            }, "LogIn Succeeded");
        }
        #endregion

        #region LogOut Async
        public Task<ApplicationServiceResult<bool>> LogOutAsync(string userId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region RefreshToken Async
        public async Task<ApplicationServiceResult<RefreshTokenResult>> RefreshTokenAsync(string refreshToken)
        {

            var userId = await _refreshTokenService.GetUserIdByRefreshTokenAsync(refreshToken);
            if (userId is null)
                return ApplicationServiceResult<RefreshTokenResult>.Fail("Invalid or expired refresh token");

            // Get User From Identity
            var user = await _userManager.FindByIdAsync(userId);
            if(user is null)
                return ApplicationServiceResult<RefreshTokenResult>.Fail("User not found");

            // Check if user is lockout
            if (await _userManager.IsLockedOutAsync(user))
                return ApplicationServiceResult<RefreshTokenResult>.Fail("Account is locked out");

            // 1. Validate old refresh token and rotate it
            var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);
            if (rotationResult is null)
                return ApplicationServiceResult<RefreshTokenResult>.Fail("Invalid or expired refresh token");

            // generate new access token
            var accessToken = await _createTokenService.CreateTokenAsync(user);

            var response = new RefreshTokenResponse
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes)
            };

            var result = new RefreshTokenResult
            {
                RefreshToken = rotationResult.RefreshToken,
                Response = response
            };

            return ApplicationServiceResult<RefreshTokenResult>.Success(result, "Token refreshed successfully");
        }
        #endregion

        #region Register Async
        public async Task<ApplicationServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest req)
        {
            req.Email = req.Email.Trim().ToLower();
            req.UserName = req.UserName.Trim().ToLower();

            // Check if email is exist
            var email = await _userManager.FindByEmailAsync(req.Email);
            if (email is not null)
                return ApplicationServiceResult<RegisterResponse>.Fail("This Email Is Already Exist");

            //check if User Name is exist
            var userName = await _userManager.FindByNameAsync(req.UserName);
            if (userName is not null)
                return ApplicationServiceResult<RegisterResponse>.Fail("This User Name Is Already Exist");

            // Mapping from Register request to Application User
            var user = _mapper.Map<ApplicationUser>(req);

            var result = await _userManager.CreateAsync(user, req.Password);

            // If Account Not Created
            if (!result.Succeeded)
                return ApplicationServiceResult<RegisterResponse>.Fail(result.Errors.Select(e => e.Description), "Account registration failed.");

            // Add Roles
            await _userManager.AddToRoleAsync(user, Roles.Visitor);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var frontUrl = _configuration["FrontEndUrl"];
            var routeUrl = _configuration["FrontEndRoute:ConfirmAccount"];
            var link = $"{frontUrl}/{routeUrl}/{user.Id}/{encodedToken}";

            // Send Email
            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm Your Account",
                $"<p>To Confirm your account click <a href='{link}'>Here</a></p>");

            var data = _mapper.Map<RegisterResponse>(user);
            return ApplicationServiceResult<RegisterResponse>.Success(data, "Account Created Succeeded You Received Confirm Email");
        }
        #endregion

    }
}
