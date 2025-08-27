using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    //[Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookings
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Event)
                .Include(b => b.Venue);

            return View(await query.ToListAsync());
        }

        // GET: Bookings/Details/5
        [AllowAnonymous] 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,CustomerId,BookingDate")] Booking booking)
        {
            
            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (ev == null)
            {
                ModelState.AddModelError(nameof(Booking.EventId), "Selected event was not found.");
            }
            else
            {
                booking.VenueId = ev.VenueId;
            }

            if (ModelState.IsValid)
            {
                booking.CreatedByUserId = _userManager.GetUserId(User);
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

           
            PopulateDropdowns(booking);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            PopulateDropdowns(booking);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,CustomerId,BookingDate")] Booking incoming)
        {
            if (id != incoming.BookingId) return NotFound();

            var existing = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (existing == null) return NotFound();

            
            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == incoming.EventId);
            if (ev == null)
            {
                ModelState.AddModelError(nameof(Booking.EventId), "Selected event was not found.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existing.EventId = incoming.EventId;
                    existing.CustomerId = incoming.CustomerId;
                    existing.BookingDate = incoming.BookingDate;
                    existing.VenueId = ev!.VenueId;                   
                    existing.UpdatedByUserId = _userManager.GetUserId(User);

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(incoming.BookingId)) return NotFound();
                    throw;
                }
            }

            PopulateDropdowns(incoming);
            return View(incoming);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

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
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id) => _context.Bookings.Any(e => e.BookingId == id);

       
        private void PopulateDropdowns(Booking? booking = null)
        {
            ViewData["EventId"] = new SelectList(_context.Events.AsNoTracking(), "EventId", "EventName", booking?.EventId);
            ViewData["CustomerId"] = new SelectList(_context.Customers.AsNoTracking(), "CustomerId", "FullName", booking?.CustomerId);
           
        }
    }
}
