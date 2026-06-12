using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10398576_EventEase.Data;
using ST10398576_EventEase.Models;
using ST10398576_EventEase.Models.ViewModels; 
using System.Linq;
using System.Threading.Tasks;

namespace ST10398576_EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? isAvailable)
        {
            var query = from b in _context.Bookings
                        join v in _context.Venues on b.VenueId equals v.VenueId
                        join e in _context.Events on b.EventId equals e.EventId
                        join et in _context.EventTypes on e.EventTypeId equals et.EventTypeId
                        select new BookingDisplayViewModel
                        {
                            BookingId = b.BookingId,
                            VenueName = v.VenueName,
                            EventName = e.EventName,
                            BookingDate = b.BookingDate,
                            Capacity = v.Capacity,
                            EventType = et.EventTypeName,
                            EventTypeId = et.EventTypeId,
                            IsAvailable = v.IsAvailable
                        };

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(q => q.EventName.Contains(searchString) || q.VenueName.Contains(searchString));

            if (eventTypeId.HasValue)
                query = query.Where(q => q.EventTypeId == eventTypeId.Value);

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(q => q.BookingDate >= startDate.Value && q.BookingDate <= endDate.Value);

            if (isAvailable.HasValue)
                query = query.Where(q => q.IsAvailable == isAvailable.Value);

            ViewBag.EventTypes = await _context.EventTypes.ToListAsync();

            return View(await query.ToListAsync());
        }
    }
}
