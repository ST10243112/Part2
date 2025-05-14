using Azure.Storage.Blobs.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseWebApplication.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        [Required]
        public string? VenueName { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }

        
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public List<Booking> Booking { get; set; } = new();        
       
    }

}
