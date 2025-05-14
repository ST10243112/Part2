using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EventEaseWebApplication.Models
{
    public class Booking
    {
     
        public int BookingId { get; set; }
        public int VenueId { get; set; }
        public int EventId { get; set; }

        //public DateOnly BookingDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        //this code is for the .includes in my iactionresult indext()
        //Entity framework core uses this to eager-load
        public Event? Event { get; set; }
         public Venue? Venue { get; set; }
      
    }
}
