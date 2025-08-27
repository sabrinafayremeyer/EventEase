using System.ComponentModel.DataAnnotations;
namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required] public DateTime BookingDate { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
