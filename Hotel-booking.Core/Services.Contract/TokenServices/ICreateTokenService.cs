using Hotel_booking.Core.Models.Accounts;

namespace Hotel_booking.Core.Services.Contract.TokenServices
{
    public interface ICreateTokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}
