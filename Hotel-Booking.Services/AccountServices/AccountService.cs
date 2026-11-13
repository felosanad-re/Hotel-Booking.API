using AutoMapper;
using Hotel_booking.Core;
using Hotel_booking.Core.Models.Accounts;
using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;
using Hotel_booking.Core.Options;
using Hotel_booking.Core.Services.Contract.AccountServices;
using Hotel_booking.Core.Services.Contract.TokenServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

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

        public AccountService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICreateTokenService createTokenService,
            IRefreshTokenService refreshTokenService,
            IOptions<JwtOptions> jwtOptions,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _createTokenService = createTokenService;
            _refreshTokenService = refreshTokenService;
            _jwtOptions = jwtOptions.Value;
            _mapper = mapper;
        }
        #endregion

        #region Login Async
        public async Task<ApplicationServiceResult<LoginResponse>> LoginAsync(LoginRequest req)
        {
            req.EmailOrUserName = req.EmailOrUserName.Trim().ToLower();

            var user = req.EmailOrUserName.Contains('@')
                ? await _userManager.FindByEmailAsync(req.EmailOrUserName)
                : await _userManager.FindByNameAsync(req.EmailOrUserName);

            if (user is null)
                return ApplicationServiceResult<LoginResponse>.Fail("User not found");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                req.Password,
                req.RememberMe,
                true
                );

            if (result.IsLockedOut)
                return ApplicationServiceResult<LoginResponse>.Fail("Email Is Lockout");
            if (result.IsNotAllowed)
                return ApplicationServiceResult<LoginResponse>.Fail("Email Not Confirm");
            if (!result.Succeeded)
                return ApplicationServiceResult<LoginResponse>.Fail("Email Or Password Is Incorrect");

            // Create Token
            var token = await _createTokenService.CreateTokenAsync(user);

            // Create Refresh Token
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            // Get user Role
            var roles = await _userManager.GetRolesAsync(user);

            // Call Set Refresh Token To Generate new Refresh Token
            SetRefreshToken(refreshToken);

            var res = new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                Email = user.Email!,
                Roles = roles,
                UserName = user.UserName!
            };
            return ApplicationServiceResult<LoginResponse>.Success(res, "LogIn Succeeded");
        }
        #endregion

        #region LogOut Async
        public Task<ApplicationServiceResult<bool>> LogOutAsync(string userId)
        {
            throw new NotImplementedException();
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


            return ApplicationServiceResult<RegisterResponse>.Success("Account Created Succeeded");
        }
        #endregion

        private void SetRefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
