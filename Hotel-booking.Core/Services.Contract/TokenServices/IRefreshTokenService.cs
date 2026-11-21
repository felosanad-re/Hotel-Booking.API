using Hotel_booking.Core.Models.Accounts;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.RefreshToken;

namespace Hotel_booking.Core.Services.Contract.TokenServices
{
    public interface IRefreshTokenService
    {
        Task<string> CreateRefreshTokenAsync(string userId);

        Task<RefreshTokenRotationResult?> RotateRefreshTokenAsync(string token);

        Task<string?> GetUserIdByRefreshTokenAsync(string token);

        Task RevokeRefreshTokenAsync(string token);
    }
}
