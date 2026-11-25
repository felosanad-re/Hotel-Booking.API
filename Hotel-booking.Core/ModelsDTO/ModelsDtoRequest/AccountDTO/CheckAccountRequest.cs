using System.ComponentModel.DataAnnotations;

namespace Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountDTO
{
    public class CheckAccountRequest
    {
        [Required]
        public string UserNameOrEmail { get; set; }
    }
}
