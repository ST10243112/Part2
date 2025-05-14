using EventEaseWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEaseWebApplication.Controllers
{
    public class EventController : Controller
    {
        private readonly EventEaseDbContextcs _context;
        public EventController(EventEaseDbContextcs context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var Event = await _context.Event
                .Include(e => e.Venue)
                .ToListAsync();
            return View(Event);
        }

        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event Event)
        {
            //it is not enough to just check for the name. As multiple events can have the same name. 
            var exists = await _context.Event.AnyAsync(e =>
                e.EventName.ToLower().Trim() == Event.EventName.ToLower().Trim() &&
                e.EventDate == Event.EventDate &&
                e.VenueId == Event.VenueId);

            if (exists)
            {
                ModelState.AddModelError("", "An event with the same name, date, and venue already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(Event);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueName", Event.VenueId);
            return View(Event);
        }

        public JsonResult GetVenue(int venueId)
        {
             var venue = _context.Venue
                 .Where(v => v.VenueId == venueId)
                 .Select(v => new { v.VenueId, v.VenueName })
                 .FirstOrDefault();

            return Json(venue);
        }

        //details action
        public async Task<IActionResult> Details(int? id)
        {
            var Event = await _context.Event.FirstOrDefaultAsync(m => m.EventId == id);
            if (Event == null)
            {
                return NotFound();
            }
            return View(Event);

        }
        //delete action
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventToDelete = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventToDelete == null) return NotFound();

            return View(eventToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventToDelete = await _context.Event.FindAsync(id);
            if (eventToDelete == null) return NotFound();

            bool hasBookings = await _context.Booking.AnyAsync(b => b.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event. It has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        private bool EventExist(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Event = await _context.Event.FindAsync(id);
            if (Event == null)
            {
                return NotFound();
            }
            return View(Event);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Event Event)
        {
            if (id != Event.EventId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Event);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Event updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExist(Event.EventId))
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
            return View(Event);
        }

    }
}
