using Hotel_booking.Core.Services.Contract.AccountServices;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Hotel_Booking.Services.AccountServices
{
    public class CurrentUserService : ICurrentUserService
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User =>
            _httpContextAccessor.HttpContext?.User;

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

        public string? UserName =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);

        public IList<string>? Roles =>
            _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(e => e.Value).ToList();
    }
}
