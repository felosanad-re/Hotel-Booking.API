namespace Hotel_booking.Core.Models
{
    public class ModelBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; }

    }
}
