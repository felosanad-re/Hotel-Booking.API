using Hotel_booking.Core.Models;

namespace Hotel_booking.Core.Specifications.RefreshTokenSpecifications
{
    public class RefreshTokenSpec : BaseSpecifications<RefreshToken>
    {
        public RefreshTokenSpec(string token)
            :base(x => x.TokenHash == token)
        {
            AddInclude(x => x.User);
        }
    }
}
