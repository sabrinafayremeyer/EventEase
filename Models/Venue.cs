using System.ComponentModel.DataAnnotations;
namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        [Required, MaxLength(200)] public string VenueName { get; set; } = "";
        [Required, MaxLength(200)] public string Location { get; set; } = "";
        public int Capacity { get; set; }
        [MaxLength(500)] public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
