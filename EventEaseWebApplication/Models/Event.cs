using System.ComponentModel.DataAnnotations;

namespace EventEaseWebApplication.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int? VenueId { get; set; }
        [Required]
        public string EventName { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public string Description { get; set; }

        // again this code is for the .includes() in my iactionresult indext()
        //Entity framework core uses this to eager-load
        public Venue? Venue { get; set; }
        public List<Booking> Booking { get; set; } = new();
    }
}
