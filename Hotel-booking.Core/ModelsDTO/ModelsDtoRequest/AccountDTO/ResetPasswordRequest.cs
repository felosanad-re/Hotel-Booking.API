using System.ComponentModel.DataAnnotations;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The Confirm Password Not Match WIth Password")]
        public string ConfirmPassword { get; set; }
    }
}
