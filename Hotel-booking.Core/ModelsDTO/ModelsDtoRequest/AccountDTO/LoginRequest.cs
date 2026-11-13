using System.ComponentModel.DataAnnotations;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(64)]
        public string EmailOrUserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
