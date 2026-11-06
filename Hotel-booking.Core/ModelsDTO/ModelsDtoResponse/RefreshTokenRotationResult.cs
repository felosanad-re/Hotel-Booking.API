using Hotel_booking.Core.Models.Accounts;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoResponse
{
    public class RefreshTokenRotationResult
    {
        public ApplicationUser User { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;
    }
}
