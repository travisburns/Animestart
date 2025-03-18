using animestart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace animestart.Controllers
{
    [Authorize(Roles = RoleConstants.Administrator)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // User Management
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);
            }
            return RedirectToAction(nameof(UserManagement));
        }

        // Anime Management
        public async Task<IActionResult> ManageAnime()
        {
            var animes = await _context.Animes.ToListAsync();
            return View(animes);
        }

        public IActionResult CreateAnime()
        {
            ViewBag.GenreTerms = _context.AnimeTerms
                .Where(t => t.Category == "Genre")
                .Select(t => t.Term)
                .ToList();

            ViewBag.EraTerms = new List<string> { "Classic", "Modern", "Contemporary" };

            ViewBag.ContentWarningTerms = _context.AnimeTerms
                .Where(t => t.Category == "Content Warning")
                .Select(t => t.Term)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnime(Anime anime, IFormFile image, string[] SelectedWarnings)
        {
            ModelState.Remove("Recommendations");

            if (image != null && image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/anime", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                anime.ImagePath = "/images/anime/" + fileName;
            }
            else
            {
                // Set a default image path or leave it null
                anime.ImagePath = "/images/default-anime.jpg";
            }

            if (SelectedWarnings != null && SelectedWarnings.Length > 0)
            {
                anime.ContentWarnings = string.Join(", ", SelectedWarnings);
            }

            if (ModelState.IsValid)
            {
                _context.Add(anime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageAnime));
            }

            // If we got to here, something failed, redisplay form
            ViewBag.GenreTerms = _context.AnimeTerms
                .Where(t => t.Category == "Genre")
                .Select(t => t.Term)
                .ToList();
            ViewBag.EraTerms = new List<string> { "Classic", "Modern", "Contemporary" };
            ViewBag.ContentWarningTerms = _context.AnimeTerms
                .Where(t => t.Category == "Content Warning")
                .Select(t => t.Term)
                .ToList();

            return View(anime);
        }
        public async Task<IActionResult> EditAnime(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var anime = await _context.Animes.FindAsync(id);
            if (anime == null)
            {
                return NotFound();
            }
            ViewBag.GenreTerms = _context.AnimeTerms
            .Where(t => t.Category == "Genre")
            .Select(t => t.Term)
            .ToList();
            ViewBag.EraTerms = new List<string> { "Classic", "Modern", "Contemporary" };
            ViewBag.ContentWarningTerms = _context.AnimeTerms
            .Where(t => t.Category == "Content Warning")
            .Select(t => t.Term)
            .ToList();
            if (!string.IsNullOrEmpty(anime.ContentWarnings))
            {
                ViewBag.SelectedWarnings = anime.ContentWarnings.Split(',')
                .Select(w => w.Trim())
                .ToList();
            }
            else
            {
                ViewBag.SelectedWarnings = new List<string>();
            }
            return View(anime);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnime(int id, Anime anime, IFormFile image, string[] SelectedWarnings)
        {
            ModelState.Remove("Recommendations");

            ModelState.Remove("image");

            if (id != anime.Id)
            {
                return NotFound();
            }


            
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    var errors = string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage));
                    Console.WriteLine($"Field: {state.Key}, Errors: {errors}");
                   
                }
            }


            // Retrieve the existing anime to get its current ImagePath
            var existingAnime = await _context.Animes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingAnime == null)
            {
                return NotFound();
            }

            // Only update ImagePath if a new image is provided, otherwise keep the existing one
            if (image != null && image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/anime", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                anime.ImagePath = "/images/anime/" + fileName;
            }
            else
            {
                // Keep the existing image path if no new image is uploaded
                anime.ImagePath = existingAnime.ImagePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (SelectedWarnings != null && SelectedWarnings.Length > 0)
                    {
                        anime.ContentWarnings = string.Join(", ", SelectedWarnings);
                    }
                    else
                    {
                        anime.ContentWarnings = string.Empty;
                    }
                    _context.Update(anime);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageAnime));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimeExists(anime.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            
            ViewBag.GenreTerms = _context.AnimeTerms
                .Where(t => t.Category == "Genre")
                .Select(t => t.Term)
                .ToList();
            ViewBag.EraTerms = new List<string> { "Classic", "Modern", "Contemporary" };
            ViewBag.ContentWarningTerms = _context.AnimeTerms
                .Where(t => t.Category == "Content Warning")
                .Select(t => t.Term)
                .ToList();

            return View(anime);
        }

        public async Task<IActionResult> DeleteAnime(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anime = await _context.Animes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (anime == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/DeleteAnime.cshtml", anime);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnime(int id)
        {
            var anime = await _context.Animes.FindAsync(id);
            if (anime != null)
            {
                _context.Animes.Remove(anime);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageAnime));
        }

        // Term Management
        public async Task<IActionResult> ManageTerms()
        {
            var terms = await _context.AnimeTerms.ToListAsync();
            return View(terms);
        }

        public IActionResult CreateTerm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerm(AnimeTerm term)
        {
            if (term.Term == null && Request.Form.ContainsKey("Term"))
            {
                term.Term = Request.Form["Term"].ToString();
            }

            if (string.IsNullOrEmpty(term.Category) && Request.Form.ContainsKey("Category"))
            {
                term.Category = Request.Form["Category"].ToString();
            }

            if (term.Definition == null && Request.Form.ContainsKey("Definition"))
            {
                term.Definition = Request.Form["Definition"].ToString();
            }

            if (!string.IsNullOrEmpty(term.Term) &&
                !string.IsNullOrEmpty(term.Category) &&
                !string.IsNullOrEmpty(term.Definition))
            {
                _context.Add(term);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageTerms));
            }

            ModelState.AddModelError("", "All fields are required.");
            return View(term);
        }

        public async Task<IActionResult> EditTerm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var term = await _context.AnimeTerms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }
            return View(term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTerm(int id, AnimeTerm term)
        {
            if (term.Term == null && Request.Form.ContainsKey("Term"))
            {
                term.Term = Request.Form["Term"].ToString();
            }

            if (string.IsNullOrEmpty(term.Category) && Request.Form.ContainsKey("Category"))
            {
                term.Category = Request.Form["Category"].ToString();
            }

            if (term.Definition == null && Request.Form.ContainsKey("Definition"))
            {
                term.Definition = Request.Form["Definition"].ToString();
            }

            term.Id = id;

            if (!string.IsNullOrEmpty(term.Term) &&
                !string.IsNullOrEmpty(term.Category) &&
                !string.IsNullOrEmpty(term.Definition))
            {
                try
                {
                    _context.Update(term);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageTerms));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "All fields are required.");
            }

            return View(term);
        }

        public async Task<IActionResult> DeleteTerm(int? id)
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

            return View("~/Views/Admin/DeleteTerm.cshtml", term);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            var term = await _context.AnimeTerms.FindAsync(id);
            if (term != null)
            {
                _context.AnimeTerms.Remove(term);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageTerms));
        }

        private bool AnimeExists(int id)
        {
            return _context.Animes.Any(e => e.Id == id);
        }

        private bool TermExists(int id)
        {
            return _context.AnimeTerms.Any(e => e.Id == id);
        }

        // Starter Pack Management
        public async Task<IActionResult> ManageStarterPacks()
        {
            var starterPacks = await _context.StarterPacks.ToListAsync();
            return View(starterPacks);
        }

        public IActionResult CreateStarterPack()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStarterPack(StarterPack starterPack)
        {
            if (ModelState.IsValid)
            {
                _context.Add(starterPack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageStarterPacks));
            }
            return View(starterPack);
        }

        public async Task<IActionResult> EditStarterPack(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var starterPack = await _context.StarterPacks.FindAsync(id);
            if (starterPack == null)
            {
                return NotFound();
            }
            return View(starterPack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStarterPack(int id, StarterPack starterPack)
        {
            if (id != starterPack.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(starterPack);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageStarterPacks));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StarterPackExists(starterPack.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(starterPack);
        }

        public async Task<IActionResult> DeleteStarterPack(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var starterPack = await _context.StarterPacks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (starterPack == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/DeleteStarterPack.cshtml", starterPack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStarterPack(int id)
        {
            var starterPack = await _context.StarterPacks
                .Include(s => s.Animes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (starterPack != null)
            {
                starterPack.Animes.Clear();
                await _context.SaveChangesAsync();

                _context.StarterPacks.Remove(starterPack);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageStarterPacks));
        }

        public async Task<IActionResult> ManageStarterPackAnime(int? id)
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

            ViewBag.AvailableAnime = await _context.Animes
                .Where(a => !starterPack.Animes.Contains(a))
                .ToListAsync();

            return View(starterPack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnimeToStarterPack(int starterPackId, int animeId)
        {
            var starterPack = await _context.StarterPacks
                .Include(s => s.Animes)
                .FirstOrDefaultAsync(s => s.Id == starterPackId);

            var anime = await _context.Animes.FindAsync(animeId);

            if (starterPack == null || anime == null)
            {
                return NotFound();
            }

            if (!starterPack.Animes.Contains(anime))
            {
                starterPack.Animes.Add(anime);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageStarterPackAnime), new { id = starterPackId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAnimeFromStarterPack(int starterPackId, int animeId)
        {
            var starterPack = await _context.StarterPacks
                .Include(s => s.Animes)
                .FirstOrDefaultAsync(s => s.Id == starterPackId);

            var anime = await _context.Animes.FindAsync(animeId);

            if (starterPack == null || anime == null)
            {
                return NotFound();
            }

            if (starterPack.Animes.Contains(anime))
            {
                starterPack.Animes.Remove(anime);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageStarterPackAnime), new { id = starterPackId });
        }

        private bool StarterPackExists(int id)
        {
            return _context.StarterPacks.Any(e => e.Id == id);
        }
    }
}