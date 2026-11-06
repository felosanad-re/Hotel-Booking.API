using System.Runtime.Serialization;

namespace Hotel_booking.Core.Models.Accounts
{
    public enum AccountStatus
    {
        [EnumMember(Value = "Active")]
        Active,

        [EnumMember(Value = "Deleted")]
        Deleted,

        [EnumMember(Value = "Suspended")]
        Suspended
    }
}
