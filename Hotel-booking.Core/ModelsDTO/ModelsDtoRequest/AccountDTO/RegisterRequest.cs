using System.ComponentModel.DataAnnotations;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices
{
    public class RegisterRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$")]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
