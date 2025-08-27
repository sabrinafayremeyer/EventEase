using System.ComponentModel.DataAnnotations;
namespace EventEase.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        [Required, MaxLength(200)] public string FullName { get; set; } = "";
        [Required, MaxLength(200), EmailAddress] public string Email { get; set; } = "";
        [MaxLength(50)] public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
