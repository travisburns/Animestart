
using animestart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace animestart.Controllers
{
    public class AnimeTermController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimeTermController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AnimeTerm
        public async Task<IActionResult> Index(string category)
        {
            // Get all categories for filter dropdown
            ViewBag.Categories = await _context.AnimeTerms
                .Select(t => t.Category)
                .Distinct()
                .ToListAsync();

            var terms = from t in _context.AnimeTerms select t;

            // Filter by category if specified
            if (!String.IsNullOrEmpty(category))
            {
                terms = terms.Where(t => t.Category == category);
            }

            return View(await terms.ToListAsync());
        }

        // GET: /AnimeTerm/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var term = await _context.AnimeTerms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (term == null)
            {
                return NotFound();
            }

            return View(term);
        }
    }
}