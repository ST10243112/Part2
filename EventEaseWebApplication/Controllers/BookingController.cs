using EventEaseWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventEaseWebApplication.Controllers
{
    public class BookingController : Controller
    {
        private readonly EventEaseDbContextcs _context;
        public BookingController(EventEaseDbContextcs context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {

            try
            {
                var Booking = _context.Booking
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    Booking = Booking.Where(b =>
                        b.Venue.VenueName.Contains(searchString) ||
                        b.Event.EventName.Contains(searchString));
                }
                return View(await Booking.ToListAsync());


            }
            catch (SqlException ex)
            {
                TempData["Message"] = "Cannot connect to the database. Please ask your administrator to allow your IP address.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }

        }

        public IActionResult Create()
        {
           ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName");
           ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Get the selected event
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            //  Check for venue conflict on the same date
            var conflict = await _context.Booking
                .Include(b => b.Event)
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
                ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            // If valid, save booking
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Booking created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "A database error occurred while saving the booking.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please contact the administrator.");
                }
            }

            // Return view with dropdowns if something failed
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }


        [HttpGet]
        public JsonResult GetVenueByEvent(int eventId)
        {
            var venueId = _context.Event
                .Where(e => e.EventId == eventId)
                .Select(e => e.VenueId)
                .FirstOrDefault();

            var venue = _context.Venue
                .Where(v => v.VenueId == venueId)
                .Select(v => new { v.VenueId, v.VenueName })
                .FirstOrDefault();

            return Json(venue);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)    
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (Booking == null)
            {
                return NotFound();
            }
            
            return View(Booking);

        }
        public async Task<IActionResult> Delete(int? id)
        {
            var Booking = await _context.Booking.FirstOrDefaultAsync(m => m.BookingId == id);
            if (Booking == null)
            {
                return NotFound();
            }
            return View(Booking);

        }
        [HttpPost]
     
        public async Task<IActionResult> Delete(int id)
        {
            var Booking = await _context.Booking.FindAsync(id);
            _context.Booking.Remove(Booking);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Booking deleted successfully!";
            return RedirectToAction(nameof(Index));

        }
        private bool BookingExist(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Booking  = await _context.Booking.FindAsync(id);
            if (Booking == null)
            {
                return NotFound();
            }

            

            return View(Booking);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking Booking)
        {
            if (id != Booking.BookingId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Booking);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Booking updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExist(Booking.BookingId))
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
            
            return View(Booking);
        }

    }

}
