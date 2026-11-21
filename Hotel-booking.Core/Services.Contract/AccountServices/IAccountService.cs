using Hotel_booking.Core.ModelsDTO;
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

        Task<ApplicationServiceResult<bool>> LogOutAsync(string userId);

        Task<ApplicationServiceResult<RefreshTokenResult>> RefreshTokenAsync(string refreshToken);

        // Confirm Email Service
    }
}
