using Hotel_booking.Core.ModelsDTO;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;

namespace Hotel_booking.Core.Services.Contract.AccountServices
{
    public interface IAccountService
    {
        // Login Service
        Task<ApplicationServiceResult<LoginResponse>> LoginAsync(LoginRequest req);

        // Register Service
        Task<ApplicationServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest req);

        Task<ApplicationServiceResult<bool>> LogOutAsync(string userId);

        // Confirm Email Service
    }
}
