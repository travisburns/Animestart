using Microsoft.EntityFrameworkCore;
using animestart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace animestart.Controllers
{
    public class AnimeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Anime
        public async Task<IActionResult> Index(string searchString, string genre, int? rating)
        {
            // Get list of genres for the filter dropdown
            ViewBag.Genres = await _context.Animes
                .Select(a => a.Genre)
                .Distinct()
                .ToListAsync();

            var animes = from a in _context.Animes select a;

            if (!String.IsNullOrEmpty(searchString))
            {
                animes = animes.Where(a => a.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(genre))
            {
                animes = animes.Where(a => a.Genre == genre);
            }

            if (rating.HasValue)
            {
                animes = animes.Where(a => a.Rating == rating.Value);
            }

            return View(await animes.ToListAsync());
        }


        // GET: /Anime/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anime = await _context.Animes
                .Include(a => a.Recommendations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anime == null)
            {
                return NotFound();
            }

            return View(anime);
        }

        // Admin-only actions
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Title,Description,Genre,Year")] Anime anime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(anime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(anime);
        }
    }
}