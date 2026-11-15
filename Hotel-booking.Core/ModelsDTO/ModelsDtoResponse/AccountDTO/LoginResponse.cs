namespace Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO
{
    public class LoginResponse
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public IList<string> Roles { get; set; }
    }
}
