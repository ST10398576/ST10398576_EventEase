using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10398576_EventEase.Data;
using ST10398576_EventEase.Models;

namespace ST10398576_EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            // load events including their venue so the view can show venue name and event date
            var events = _context.Events.Include(e => e.Venue).ToList();
            ViewBag.Events = events;
            ViewData["EventId"] = new SelectList(events, "EventId", "EventName");
            return View();
        }

        // POST: Bookings/Create
        // Only bind BookingId and EventId; BookingDate and VenueId are derived from the selected Event.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId")] Booking booking)
        {
            // Find the selected event (includes EventDate and Venue)
            var ev = await _context.Events.AsNoTracking().Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (ev == null)
            {
                ModelState.AddModelError("EventId", "Selected event was not found. Please choose an event.");
            }
            else
            {
                // set booking properties based on the event (server-side authoritative)
                booking.BookingDate = ev.EventDate;
                booking.VenueId = ev.VenueId;

                // remove validation entries that complain about Event/Venue being required
                ModelState.Remove(nameof(booking.EventId));
                ModelState.Remove(nameof(booking.VenueId));
                ModelState.Remove("Event");
                ModelState.Remove("Venue");
            }

            // Now validate remaining properties (e.g. BookingDate)
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // repopulate events for redisplay
            var events = _context.Events.Include(e => e.Venue).ToList();
            ViewBag.Events = events;
            ViewData["EventId"] = new SelectList(events, "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // provide events with venue info for the edit view (for readonly displays)
            var events = _context.Events.Include(e => e.Venue).ToList();
            ViewBag.Events = events;
            ViewData["EventId"] = new SelectList(events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,BookingDate,EventId,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // ensure VenueId matches selected Event (defense in depth)
            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (ev != null)
            {
                booking.VenueId = ev.VenueId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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

            // repopulate events and venues for redisplay
            var events = _context.Events.Include(e => e.Venue).ToList();
            ViewBag.Events = events;
            ViewData["EventId"] = new SelectList(events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
