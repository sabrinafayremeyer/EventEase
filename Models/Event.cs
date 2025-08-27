using System.ComponentModel.DataAnnotations;
namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }
        [Required, MaxLength(200)] public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
