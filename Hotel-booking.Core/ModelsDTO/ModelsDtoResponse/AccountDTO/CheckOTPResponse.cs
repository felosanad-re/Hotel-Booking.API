namespace Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO
{
    public class CheckOTPResponse
    {
        public bool IsValid { get; set; }
        public string Token { get; set; }
    }
}
