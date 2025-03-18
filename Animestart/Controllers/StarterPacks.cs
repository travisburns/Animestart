using animestart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace animestart.Controllers
{
    public class StarterPackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StarterPackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /StarterPack
        public async Task<IActionResult> Index()
        {
            var starterPacks = await _context.StarterPacks.ToListAsync();
            return View(starterPacks);
        }

        // GET: /StarterPack/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var starterPack = await _context.StarterPacks
                .Include(s => s.Animes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (starterPack == null)
            {
                return NotFound();
            }

            // If starter pack has no anime, we'll still show some recommended ones
            if (starterPack.Animes == null || !starterPack.Animes.Any())
            {
                ViewBag.RecommendedAnime = await _context.Animes
                    .Where(a => a.Rating <= 3) // Just a sample filter
                    .Take(4)
                    .ToListAsync();
            }

            return View(starterPack);
        }
    }
}