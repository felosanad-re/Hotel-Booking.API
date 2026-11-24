using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.RefreshToken;

namespace Hotel_booking.Core.Services.Contract.AccountServices
{
    public interface IAccountService
    {
        // Login Service
        Task<ApplicationServiceResult<LoginResult>> LoginAsync(LoginRequest req);

        // Register Service
        Task<ApplicationServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest req);

        Task<ApplicationServiceResult<bool>> LogOutAsync(string refreshToken);

        Task<ApplicationServiceResult<RefreshTokenResult>> RefreshTokenAsync(string refreshToken);

        // Confirm Email Service
        Task<ApplicationServiceResult<bool>> ConfirmAccount(ConfirmAccountRequest req);

        Task<ApplicationServiceResult<CheckAccountResponse>> CheckAccountAsync(CheckAccountRequest req);
        Task<ApplicationServiceResult<CheckOTPResponse>> CheckOTP(CheckOTPRequest req);
        Task<ApplicationServiceResult<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest req);
        Task<ApplicationServiceResult<CheckEmailConfirmationResponse>> CheckEmailConfirmationAsync(CheckEmailConfirmationRequest req);
    }
}
