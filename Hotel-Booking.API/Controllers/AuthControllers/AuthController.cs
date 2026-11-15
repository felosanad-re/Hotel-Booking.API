using Hotel_booking.Core.ModelsDTO;
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

        [HttpPost("login")] // Post: /api/Auth/Login
        public async Task<ActionResult<ApplicationServiceResult<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var res = await _accountService.LoginAsync(request);
            if (!res.Succeed || res.Data is null)
                return BadRequest(new ErrorResponse(400));
            Response.SetRefreshToken(res.Data.RefreshToken);
            return Ok(res.Data.Response);
        }

        [HttpPost("Register")] // Post: /api/Auth/Register
        public async Task<ActionResult<ApplicationServiceResult<RegisterResponse>>> Register([FromBody] RegisterRequest register)
        {
            var res = await _accountService.RegisterAsync(register);

            if (!res.Succeed || res.Data is null)
                return BadRequest(new ErrorResponse(400));

            return Ok(res.Data);
        }
    }
}
