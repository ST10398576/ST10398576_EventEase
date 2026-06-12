using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10398576_EventEase.Data;
using ST10398576_EventEase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST10398576_EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType); // NEW
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // NEW
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        // GET: Events/Create
        public IActionResult Create()
        {
            ViewBag.VenueId = new SelectList(_context.Venues.Where(v => v.IsAvailable), "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName");
            return View();
        }

        // POST: Events/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId,EventTypeId")] Event @event)
        {
            if (@event != null)
            {
                // Ensure the chosen EventType exists
                bool eventTypeExists = await _context.EventTypes.AnyAsync(t => t.EventTypeId == @event.EventTypeId);
                if (!eventTypeExists)
                {
                    ModelState.AddModelError("EventTypeId", "Please select a valid Event Type.");
                }

                // Prevent double-booking: check for same venue + same calendar date
                bool conflict = await _context.Events
                    .AnyAsync(e => e.VenueId == @event.VenueId
                                   && e.EventDate.Year == @event.EventDate.Year
                                   && e.EventDate.Month == @event.EventDate.Month
                                   && e.EventDate.Day == @event.EventDate.Day);

                if (conflict)
                {
                    var msg = "An event already exists at the selected venue on that date. Please choose a different date or venue.";
                    ModelState.AddModelError("VenueId", msg);
                    ModelState.AddModelError("EventDate", msg);
                    ModelState.AddModelError(string.Empty, msg);
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine(string.Join(", ", errors));
            }

            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VenueId = new SelectList(_context.Venues.Where(v => v.IsAvailable), "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            ViewBag.VenueId = new SelectList(_context.Venues.Where(v => v.IsAvailable), "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId,EventTypeId")] Event @event)
        {
            if (id != @event.EventId) return NotFound();
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine(string.Join(", ", errors)); // or log it
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    else throw;
                }
            }

            ViewBag.VenueId = new SelectList(_context.Venues.Where(v => v.IsAvailable), "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            //Restrict deletion if bookings exist
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete a event with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
