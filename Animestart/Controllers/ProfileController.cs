
using animestart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace animestart.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var recommendations = await _context.UserRecommendations
                .Include(r => r.Anime)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var watchlistItems = await _context.WatchlistItems
                .Include(w => w.Anime)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Recommendations = recommendations,
                WatchlistItems = watchlistItems,
                ActiveTab = "Watchlist"
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile profileImage)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (profileImage != null && profileImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/images/profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWatchlist(int animeId)
        {
            var userId = _userManager.GetUserId(User);
            var exists = await _context.WatchlistItems.AnyAsync(w => w.AnimeId == animeId && w.UserId == userId);

            if (!exists)
            {
                var watchlistItem = new UserWatchlistItem
                {
                    AnimeId = animeId,
                    UserId = userId,
                    AddedDate = DateTime.Now
                };

                _context.WatchlistItems.Add(watchlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Anime", new { id = animeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWatchlist(int animeId)
        {
            var userId = _userManager.GetUserId(User);
            var watchlistItem = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.AnimeId == animeId && w.UserId == userId);

            if (watchlistItem != null)
            {
                _context.WatchlistItems.Remove(watchlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}