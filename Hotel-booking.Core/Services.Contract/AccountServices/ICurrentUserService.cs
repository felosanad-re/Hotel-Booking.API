namespace Hotel_booking.Core.Services.Contract.AccountServices
{
    public interface ICurrentUserService
    {
        public string? UserId { get; }
        public string? Email { get; }
        public string? UserName { get; }
        public IList<string>? Roles { get; }
    }
}
