using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;
using Hotel_booking.Core.Services.Contract.AccountServices;
using Hotel_Booking.API.Error_Handler;
using Hotel_Booking.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking.API.Controllers.AuthControllers
{
    public class AuthController : BaseController
    {
        #region Inject Services
        protected readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        #endregion

        #region login
        [HttpPost("login")] // Post: /api/Auth/Login
        public async Task<ActionResult<ApplicationServiceResult<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var res = await _accountService.LoginAsync(request);
            if (!res.Succeed || res.Data is null)
                return BadRequest(new ErrorResponse(400));
            Response.SetRefreshToken(res.Data.RefreshToken);
            return Ok(res.Data.Response);
        }
        #endregion

        #region Register
        [HttpPost("Register")] // Post: /api/Auth/Register
        public async Task<ActionResult<ApplicationServiceResult<RegisterResponse>>> Register([FromBody] RegisterRequest register)
        {
            var res = await _accountService.RegisterAsync(register);

            if (!res.Succeed || res.Data is null)
                return BadRequest(new ErrorResponse(400));

            return Ok(res.Data);
        }
        #endregion

        #region LogOut
        [HttpPost("logout")] //Post: /api/auth/logout
        public async Task<IActionResult> LogOut()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _accountService.LogOutAsync(refreshToken
                    );

            Response.Cookies.Delete("refreshToken");

            return Ok(new
            {
                Message = "Logged out successfully"
            });
        }
        #endregion

        #region Check Account
        [HttpPost("CheckAccount")] // POST: /api/Account/CheckAccount
        public async Task<ActionResult<ApplicationServiceResult<CheckAccountResponse>>> Check(CheckAccountRequest req)
        {
            var result = await _accountService.CheckAccountAsync(req);
            if (!result.Succeed) return BadRequest(new { message = result.Message });

            return Ok(result);
        }
        #endregion

        #region Check OTP
        [HttpPost("CheckOTP")] // POST: /api/Account/CheckOTP
        public async Task<ActionResult<ApplicationServiceResult<CheckOTPResponse>>> CheckOTP(CheckOTPRequest req)
        {
            var result = await _accountService.CheckOTP(req);
            if (!result.Succeed) return BadRequest(new { message = result.Message });

            return Ok(result);
        }
        #endregion

        #region Reset Password
        [HttpPost("ResetPassword")] // POST: /api/Account/ResetPassword
        public async Task<ActionResult<ApplicationServiceResult<ResetPasswordResponse>>> ResetPassword(ResetPasswordRequest req)
        {
            var result = await _accountService.ResetPasswordAsync(req);
            if (!result.Succeed) return BadRequest(new { message = result.Message });

            return Ok(result);
        }
        #endregion

        #region Check Email Confirmation
        [HttpPost("CheckEmailConfirmation")] // POST: /api/Account/CheckEmailConfirmation
        public async Task<ActionResult<ApplicationServiceResult<CheckEmailConfirmationResponse>>> CheckEmailConfirmation(CheckEmailConfirmationRequest req)
        {
            var result = await _accountService.CheckEmailConfirmationAsync(req);
            if (!result.Succeed) return BadRequest(new { message = result.Message });

            return Ok(result);
        }
        #endregion

        #region Register
        [HttpPost("Register")] // POST: /api/Account/Register
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApplicationServiceResult<RegisterResponse>>> CreateAccount([FromBody] RegisterRequest req)
        {
            var result = await _accountService.RegisterAsync(req);
            if (!result.Succeed)
            {
                if (result.Message?.Contains("Already Exist", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Conflict(new { message = result.Message, error = result.Errors });
                }
                ;

                return BadRequest(new { message = result.Message, errors = result.Errors });
            }
            return Ok(result);
        }
        #endregion

        #region Confirm
        [HttpGet("Confirm")] // GET: /api/Account/Confirm
        public async Task<ActionResult<ApplicationServiceResult<bool>>> Confirm([FromQuery] ConfirmAccountRequest request)
        {
            var result = await _accountService.ConfirmAccount(request);
            if (!result.Succeed)
                return BadRequest(result.Message);

            return Ok(result);
        }
        #endregion
    }
}
