using System.ComponentModel.DataAnnotations;

namespace ST10398576_EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Be sure to choose a Venue for the Event.")]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Required(ErrorMessage = "Be sure to choose the Type of Event.")]
        public int EventTypeId { get; set; }
        // navigation property should be nullable so model binding/validation doesn't
        // require the entire EventType object to be present when posting the form
        public EventType? EventType { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
