using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    //[Authorize] 
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var eventsWithVenue = _context.Events.Include(e => e.Venue);
            return View(await eventsWithVenue.ToListAsync());
        }

        // GET: Events/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            PopulateVenueDropdown();
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,Description,StartDateTime,EndDateTime,VenueId,IsActive,ImageUrl")] Event ev)
        {
            if (ev.StartDateTime.HasValue && ev.EndDateTime.HasValue && ev.StartDateTime > ev.EndDateTime)
                ModelState.AddModelError(nameof(Event.EndDateTime), "End time must be after start time.");

            if (ModelState.IsValid)
            {
                if (await VenueHasClashAsync(ev.VenueId, ev.StartDateTime, ev.EndDateTime))
                {
                   
                    ModelState.AddModelError(nameof(Event.StartDateTime), "This venue is already booked for the selected time.");
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateVenueDropdown(ev.VenueId);
                return View(ev);
            }

            _context.Add(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            PopulateVenueDropdown(ev.VenueId);
            return View(ev);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Description,StartDateTime,EndDateTime,VenueId,IsActive,ImageUrl")] Event incoming)
        {
            if (id != incoming.EventId) return NotFound();

            if (incoming.StartDateTime.HasValue && incoming.EndDateTime.HasValue && incoming.StartDateTime > incoming.EndDateTime)
                ModelState.AddModelError(nameof(Event.EndDateTime), "End time must be after start time.");

            if (ModelState.IsValid)
            {
                if (await VenueHasClashAsync(incoming.VenueId, incoming.StartDateTime, incoming.EndDateTime, ignoreEventId: id))
                {
                    ModelState.AddModelError(nameof(Event.StartDateTime), "This venue is already booked for the selected time.");
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateVenueDropdown(incoming.VenueId);
                return View(incoming);
            }

            var existing = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (existing == null) return NotFound();

            existing.EventName = incoming.EventName;
            existing.Description = incoming.Description;
            existing.StartDateTime = incoming.StartDateTime;
            existing.EndDateTime = incoming.EndDateTime;
            existing.VenueId = incoming.VenueId;
            existing.IsActive = incoming.IsActive;
            existing.ImageUrl = incoming.ImageUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id) => _context.Events.Any(e => e.EventId == id);

        private void PopulateVenueDropdown(int? selectedVenueId = null)
        {
            ViewData["VenueId"] = new SelectList(
                _context.Venues.AsNoTracking(),
                "VenueId",
                "VenueName",   //show name, not ID/Location
                selectedVenueId
            );
        }
        private async Task<bool> VenueHasClashAsync(int venueId, DateTime? start, DateTime? end, int? ignoreEventId = null)
        {
            if (!start.HasValue || !end.HasValue) return false; //if either is null, skip clash logic
            var s = start.Value;
            var e = end.Value;

           
            return await _context.Events
                .AsNoTracking()
                .AnyAsync(x =>
                    x.VenueId == venueId &&
                    x.EventId != (ignoreEventId ?? 0) &&
                    x.StartDateTime.HasValue && x.EndDateTime.HasValue &&
                    x.StartDateTime.Value < e &&
                    s < x.EndDateTime.Value
                );
        }

    }
}
