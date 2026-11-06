using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse;
using Hotel_booking.Core.Services.Contract.TokenServices;

namespace Hotel_Booking.Services.TokenServices
{
    public class RefreshTokenService : IRefreshTokenService
    {

        public Task<string> CreateRefreshTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<RefreshTokenRotationResult> RotateRefreshTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRefreshTokenAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
