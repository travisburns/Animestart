using animestart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace animestart.Controllers
{
    [Authorize]  // Requires user to be logged in
    public class RecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecommendationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int animeId, string comment, int rating)
        {
            var userId = _userManager.GetUserId(User);

            var recommendation = new UserRecommendation
            {
                AnimeId = animeId,
                UserId = userId,
                Comment = comment,
                Rating = rating
            };

            if (ModelState.IsValid)
            {
                _context.UserRecommendations.Add(recommendation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Anime", new { id = animeId });
            }

            return RedirectToAction("Details", "Anime", new { id = animeId });
        }

        [Authorize]
        public async Task<IActionResult> MyRecommendations()
        {
            var userId = _userManager.GetUserId(User);
            var recommendations = await _context.UserRecommendations
                .Include(r => r.Anime)
                .Where(r => r.UserId == userId)
                .ToListAsync();
            return View(recommendations);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var recommendation = await _context.UserRecommendations
                .Include(r => r.Anime) 
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation == null)
            {
                return NotFound();
            }

            return View(recommendation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AnimeId,Rating,Comment")] UserRecommendation recommendation)
        {

            ModelState.Remove("Anime");
            ModelState.Remove("UserId");


        
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    var errors = string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage));
                    Console.WriteLine($"Field: {state.Key}, Errors: {errors}");
                }
            }

            if (id != recommendation.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            recommendation.UserId = userId;

            var existingRecommendation = await _context.UserRecommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (existingRecommendation == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingRecommendation.Rating = recommendation.Rating;
                    existingRecommendation.Comment = recommendation.Comment;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Profile");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                }
            }

            // Reload the associated anime for the view
            recommendation.Anime = await _context.Animes.FindAsync(recommendation.AnimeId);
            return View(recommendation);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var recommendation = await _context.UserRecommendations
                .Include(r => r.Anime)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation == null)
            {
                return NotFound();
            }

            return View(recommendation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var recommendation = await _context.UserRecommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation != null)
            {
                _context.UserRecommendations.Remove(recommendation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyRecommendations));
        }



    }
}
