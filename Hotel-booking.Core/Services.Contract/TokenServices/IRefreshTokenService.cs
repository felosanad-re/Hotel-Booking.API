using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse;

namespace Hotel_booking.Core.Services.Contract.TokenServices
{
    public interface IRefreshTokenService
    {
        Task<string> CreateRefreshTokenAsync(string userId);

        Task<RefreshTokenRotationResult> RotateRefreshTokenAsync(string token);

        Task RevokeRefreshTokenAsync(string token);
    }
}
