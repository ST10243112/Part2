using EventEaseWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using static System.Net.WebRequestMethods;

namespace EventEaseWebApplication.Controllers
{
    public class VenueController : Controller
    {
        private readonly EventEaseDbContextcs _context;

        public VenueController(EventEaseDbContextcs context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var Venue = await _context.Venue.ToListAsync();
            return View(Venue);
        }
        public IActionResult Create() //creates a view 
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue Venue)
        {
            var exists = await _context.Venue
                .AnyAsync(v => v.VenueName.ToLower() == Venue.VenueName.ToLower());

            if (exists)
            {
                ModelState.AddModelError("", "A venue with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                if (Venue.ImageFile != null)
                {
                    var blobUrl = await UploadImageToBlobAsync(Venue.ImageFile);  //UploadImageToBlobAsync(Venue.ImageFile);

                    Venue.ImageUrl = blobUrl;
                }
                _context.Add(Venue);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Venue created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(Venue);
        }
        //details action
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);

            if (Venue == null)
            {
                return NotFound();
            }
            return View(Venue);

        }

        //delete action 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);

            if (Venue == null)
            {
                return NotFound();
            }
            return View(Venue);

        }

        //updtaes venue list based on delete action 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            bool hasEvents = await _context.Event.AnyAsync(e => e.VenueId == id);
            bool hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

            if (hasEvents || hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue. It is associated with events or bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        //action for edit 
        private bool VenueExist(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Venue = await _context.Venue.FindAsync(id);
            if (Venue == null)
            {
                return NotFound();
            }
            return View(Venue);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Venue Venue)
        {
            if (id != Venue.VenueId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAsync(Venue.ImageFile);
                        Venue.ImageUrl = blobUrl;
                    }
                    else
                    {
                        var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                        if (existingVenue != null)
                        {
                            Venue.ImageUrl = existingVenue.ImageUrl; // Keep the existing image URL
                        }

                    }
                    _context.Update(Venue);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Venue updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExist(Venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Venue);
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            string connectionString = "";
            string containerName = "imagecontainer";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = imageFile.ContentType
                },
                Metadata = new Dictionary<string, string>

                {

                  { "UploadedBy", "EventEase" }

                }
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, uploadOptions);
            }

            return blobClient.Uri.ToString();
        }

    }
}
