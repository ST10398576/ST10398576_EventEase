using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST10398576_EventEase.Models;

namespace ST10398576_EventEase.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // apply migrations (if any) and ensure database exists
            context.Database.Migrate();

            // If any venues exist, assume DB has been seeded
            if (context.Venues.Any())
            {
                return;
            }

            // Seed venues
            var venues = new[]
            {
                new Venue
                {
                    VenueName = "Community Hall",
                    Location = "123 Main St",
                    Capacity = 200,
                    ImageUrl = "https://via.placeholder.com/300x200.png?text=Community+Hall"
                },
                new Venue
                {
                    VenueName = "City Stadium",
                    Location = "456 Stadium Rd",
                    Capacity = 5000,
                    ImageUrl = "https://via.placeholder.com/300x200.png?text=City+Stadium"
                },
                new Venue
                {
                    VenueName = "Conference Center",
                    Location = "789 Conference Blvd",
                    Capacity = 800,
                    ImageUrl = "https://via.placeholder.com/300x200.png?text=Conference+Center"
                }
            };

            context.Venues.AddRange(venues);
            context.SaveChanges();

            // Seed events (assign VenueId from saved venues)
            var events = new[]
            {
                new Event
                {
                    EventName = "Spring Concert",
                    EventDate = DateTime.UtcNow.Date.AddDays(14),
                    Description = "A local spring concert featuring community bands.",
                    VenueId = venues[0].VenueId
                },
                new Event
                {
                    EventName = "City Marathon Expo",
                    EventDate = DateTime.UtcNow.Date.AddDays(30),
                    Description = "Expo for marathon participants and vendors.",
                    VenueId = venues[1].VenueId
                },
                new Event
                {
                    EventName = "Tech Conference",
                    EventDate = DateTime.UtcNow.Date.AddDays(45),
                    Description = "Two-day conference on modern web development.",
                    VenueId = venues[2].VenueId
                }
            };

            context.Events.AddRange(events);
            context.SaveChanges();

            // Seed a couple of bookings
            var bookings = new[]
            {
                new Booking
                {
                    BookingDate = events[0].EventDate,
                    EventId = events[0].EventId,
                    VenueId = events[0].VenueId
                },
                new Booking
                {
                    BookingDate = events[2].EventDate,
                    EventId = events[2].EventId,
                    VenueId = events[2].VenueId
                }
            };

            context.Bookings.AddRange(bookings);
            context.SaveChanges();
        }
    }
}