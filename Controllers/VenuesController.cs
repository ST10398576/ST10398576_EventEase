using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10398576_EventEase.Data;
using ST10398576_EventEase.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST10398576_EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public VenuesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile imageFile)
        {
            // 🔹 Enforce uploads check
            if (imageFile == null || imageFile.Length == 0)
            {
                // Associate validation error with the model property so the view's
                // asp-validation-for="ImageUrl" element displays the message like other fields.
                ModelState.AddModelError("ImageUrl", "Please upload an image.");
            }
            else
            {
                // Connect to Blob Storage
                var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureStorage"));
                var containerClient = blobServiceClient.GetBlobContainerClient("eventeaseimages");
                await containerClient.CreateIfNotExistsAsync();

                // Upload file
                var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName));
                using (var stream = imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // Save Blob URL to DB
                venue.ImageUrl = blobClient.Uri.ToString();
            }

            // Note: validation for missing image is already added above (ImageUrl key)

            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }


        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile imageFile)
        {
            // Load existing entity so we only change the fields the user updated.
            var existing = await _context.Venues.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Ensure file-related validation doesn't block editing when user chooses not to upload a new image
            ModelState.Remove("ImageUrl");
            ModelState.Remove("imageFile");

            // Update only the allowed properties from the incoming form onto the tracked entity
            var updated = await TryUpdateModelAsync<Venue>(existing, "", v => v.VenueName, v => v.Location, v => v.Capacity, v => v.IsAvailable);

            // If a new image was uploaded, upload and replace the ImageUrl; otherwise keep existing.ImageUrl
            if (imageFile != null && imageFile.Length > 0)
            {
                var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureStorage"));
                var containerClient = blobServiceClient.GetBlobContainerClient("eventeaseimages");
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName));
                using (var stream = imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }
                existing.ImageUrl = blobClient.Uri.ToString();
            }

            if (updated && ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(existing.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed; return the view with the existing entity so the image is displayed
            return View(existing);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            //Restrict deletion if bookings exist
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
            if(hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete a venue with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (venue != null)
            {
                _context.Venues.Remove(venue);
                
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}
