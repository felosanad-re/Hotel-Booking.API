using System.ComponentModel.DataAnnotations;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO
{
    public class CheckOTPRequest
    {
        [Required]
        public int OTP { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
