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

        public async Task<IActionResult> Index(string searchString)
        {
            var query = from b in _context.Bookings
                        join v in _context.Venues on b.VenueId equals v.VenueId
                        join e in _context.Events on b.EventId equals e.EventId
                        select new BookingDisplayViewModel
                        {
                            BookingId = b.BookingId,
                            VenueName = v.VenueName,
                            EventName = e.EventName,
                            BookingDate = b.BookingDate,
                            Capacity = v.Capacity
                        };

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q => q.EventName.Contains(searchString)
                                      || q.VenueName.Contains(searchString)
                                      || q.BookingId.ToString().Contains(searchString));
            }

            return View(await query.ToListAsync());
        }
    }
}
