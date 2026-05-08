using System.ComponentModel.DataAnnotations;

namespace ST10398576_EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required(ErrorMessage ="Venue name is required.")]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Venue location is required.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Venue capacity is required. Capacity must be between 1 and 10,000 people.")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000 people.")]
        public int Capacity { get; set; }

        //Controller sets this after upload
        public string? ImageUrl { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
