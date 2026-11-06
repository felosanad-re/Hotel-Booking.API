using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_booking.Core.Models.Accounts
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        // Date of birth used to calculate Age dynamically
        public DateTime Birthday { get; set; }

        // Soft delete flag — separate from Identity's LockoutEnd
        public bool IsDeleted { get; set; }

        public AccountStatus Status { get; set; } = AccountStatus.Active;

        public DateTime? SuspendedAt { get; set; }

        public string? SuspendedBy { get; set; } // Admin Id

        public string? SuspensionReason { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; } // Admin Id

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
