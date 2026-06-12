namespace ST10398576_EventEase.Models.ViewModels
{
    public class BookingDisplayViewModel
    {
        public int BookingId { get; set; }
        public string VenueName { get; set; }
        public string EventName { get; set; }
        public DateTime BookingDate { get; set; }
        public int Capacity { get; set; }
        public int EventTypeId { get; set; }
        public string EventType { get; set; }
        public bool IsAvailable { get; set; } 
    }
}
