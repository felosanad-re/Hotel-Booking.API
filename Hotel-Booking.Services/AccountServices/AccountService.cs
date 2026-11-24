using AutoMapper;
using Hotel_booking.Core;
using Hotel_booking.Core.GenericRepository;
using Hotel_booking.Core.Models.Accounts;
using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO;
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
using System.Security.Cryptography;
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
        protected readonly IRedisRepo<OTPUser> _redisRepo;

        public AccountService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICreateTokenService createTokenService,
            IRefreshTokenService refreshTokenService,
            IOptions<JwtOptions> jwtOptions,
            IMapper mapper,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _createTokenService = createTokenService;
            _refreshTokenService = refreshTokenService;
            _jwtOptions = jwtOptions.Value;
            _mapper = mapper;
            _configuration = configuration;
            _emailSender = emailSender;
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
        public async Task<ApplicationServiceResult<bool>> LogOutAsync(string refreshToken)
        {
            if(string.IsNullOrWhiteSpace(refreshToken))
                return ApplicationServiceResult<bool>.Fail("refresh token is required");

            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);

            return ApplicationServiceResult<bool>.Success(true, "Logged out successfully");
        }
        #endregion

        #region RefreshToken Async
        public async Task<ApplicationServiceResult<RefreshTokenResult>> RefreshTokenAsync(string refreshToken)
        {
            // 1. Validate old refresh token and rotate it
            var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);
            if (rotationResult is null)
                return ApplicationServiceResult<RefreshTokenResult>.Fail("Invalid or expired refresh token");

            // Get User From rotation result
            var user = rotationResult.User;

            // check if user is exist
            if (user is null)
                return ApplicationServiceResult<RefreshTokenResult>.Fail("User Not Found");

            // Check if user is lockout
            if (await _userManager.IsLockedOutAsync(user))
                return ApplicationServiceResult<RefreshTokenResult>.Fail("Account is locked out");

            // generate new access token
            var accessToken = await _createTokenService.CreateTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

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

        public async Task<ApplicationServiceResult<bool>> ConfirmAccount(ConfirmAccountRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null) return ApplicationServiceResult<bool>.Fail("Invalid Account");

            // Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Token));
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded) return ApplicationServiceResult<bool>.Fail("Invalid or expired token");

            return ApplicationServiceResult<bool>.Success(true, "Account activated Succeeded");
        }
        #endregion

        #region CheckAccountAsync
        public async Task<ApplicationServiceResult<CheckAccountResponse>> CheckAccountAsync(CheckAccountRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserNameOrEmail))
                return ApplicationServiceResult<CheckAccountResponse>.Fail("UserNameOrEmail is required");

            var user = req.UserNameOrEmail.Contains('@') ?
            await _userManager.FindByEmailAsync(req.UserNameOrEmail) :
            await _userManager.FindByNameAsync(req.UserNameOrEmail);

            if (user == null) return ApplicationServiceResult<CheckAccountResponse>.Fail("User Not Found");

            var token = await GenerateAndStoreOTPAsync(user.Id, user.Email!);

            var result = new CheckAccountResponse()
            {
                Exists = true,
                CanResetPassword = user.EmailConfirmed,
                RequiresOTP = true,
                Token = token
            };
            return ApplicationServiceResult<CheckAccountResponse>.Success(result, "Email Is Exist You Received OTP");
        }
        #endregion

        #region CheckOTP
        public async Task<ApplicationServiceResult<CheckOTPResponse>> CheckOTP(CheckOTPRequest req)
        {
            var key = $"OTP:{req.Token}";
            var userOTP = await _redisRepo.GetKeyAsync(key);

            if (userOTP == null)
                return ApplicationServiceResult<CheckOTPResponse>.Fail("Invalid or expired token, please request a new OTP");

            if (userOTP.IsExpired(TimeSpan.FromMinutes(5)))
            {
                await _redisRepo.DeleteKeyAsync(key);
                return ApplicationServiceResult<CheckOTPResponse>.Fail("OTP has expired, please request a new one");
            }

            if (userOTP.OTP != req.OTP)
                return ApplicationServiceResult<CheckOTPResponse>.Fail("Invalid OTP code");

            // Mark as verified and update in Redis
            userOTP.IsVerified = true;
            await _redisRepo.SetKeyAsync(key, userOTP, TimeSpan.FromMinutes(10));

            var result = new CheckOTPResponse
            {
                IsValid = true,
                Token = userOTP.Token
            };
            return ApplicationServiceResult<CheckOTPResponse>.Success(result, "OTP verified successfully");
        }
        #endregion

        #region ResetPasswordAsync
        public async Task<ApplicationServiceResult<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest req)
        {
            var key = $"OTP:{req.Token}";
            var userOTP = await _redisRepo.GetKeyAsync(key);

            if (userOTP == null)
                return ApplicationServiceResult<ResetPasswordResponse>.Fail("Invalid or expired token, please start over");

            if (!userOTP.IsVerified)
                return ApplicationServiceResult<ResetPasswordResponse>.Fail("OTP not verified, please verify your OTP first");

            var user = await _userManager.FindByIdAsync(userOTP.UserId);
            if (user == null)
                return ApplicationServiceResult<ResetPasswordResponse>.Fail("User not found");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, req.Password);

            if (!result.Succeeded)
                return ApplicationServiceResult<ResetPasswordResponse>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Clean up OTP from Redis after successful reset
            await _redisRepo.DeleteKeyAsync(key);

            var response = new ResetPasswordResponse
            {
                UserId = user.Id,
                Email = user.Email!
            };
            return ApplicationServiceResult<ResetPasswordResponse>.Success(response, "Password reset successfully");
        }

        #endregion

        #region Check Email Confirmation
        public async Task<ApplicationServiceResult<CheckEmailConfirmationResponse>> CheckEmailConfirmationAsync(CheckEmailConfirmationRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserNameOrEmail))
                return ApplicationServiceResult<CheckEmailConfirmationResponse>.Fail("Email is required");

            var user = await _userManager.FindByEmailAsync(req.UserNameOrEmail.Trim().ToLower());
            if (user == null)
                return ApplicationServiceResult<CheckEmailConfirmationResponse>.Fail("Account not found");

            // If email is not confirmed, send confirmation email
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var frontUrl = _configuration["FrontEndUrl"];
                var routeUrl = _configuration["FrontEndRoute:ConfirmAccount"];
                var link = $"{frontUrl}/{routeUrl}/{user.Id}/{encodedToken}";

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm Your Account",
                    $"<p>To Confirm your account click <a href='{link}'>Here</a></p>"
                );
            }

            var response = new CheckEmailConfirmationResponse
            {
                Exists = true,
                IsConfirmed = user.EmailConfirmed
            };

            var message = user.EmailConfirmed
                ? "Email is confirmed"
                : "Confirmation email has been sent";

            return ApplicationServiceResult<CheckEmailConfirmationResponse>.Success(response, message);
        }
        #endregion

        #region Private Helper Methods
        private async Task<string> GenerateAndStoreOTPAsync(string userId, string email)
        {
            var OTP = RandomNumberGenerator.GetInt32(100000, 999999); // 6 Digits
            var token = Guid.NewGuid().ToString("N");
            var userOTP = new OTPUser
            {
                OTP = OTP,
                Token = token,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Set OTP In Redis using Token as key (so CheckOTP can look it up)
            var key = $"OTP:{token}";
            await _redisRepo.SetKeyAsync(key, userOTP, TimeSpan.FromMinutes(5));

            var htmlMessage = $@"
                <h2>Password Reset Request</h2>
                <p>Your OTP Code: <strong>{OTP}</strong></p>
                <p><small>This code expires in 5 minutes.</small></p>";

            await _emailSender.SendEmailAsync(email, "Forget Password", htmlMessage);

            return token;
        }
        #endregion
    }
}
