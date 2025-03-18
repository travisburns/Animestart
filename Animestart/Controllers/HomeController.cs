using animestart.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace animestart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            
            if (User.Identity.IsAuthenticated)
            {
              
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    // Get roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Store in ViewBag
                    ViewBag.UserRoles = roles;
                    ViewBag.IsAdmin = User.IsInRole(RoleConstants.Administrator);
                    ViewBag.UserId = user.Id;
                    ViewBag.UserName = user.UserName;

                    // Check if role exists at all
                    ViewBag.AdminRoleExists = await _roleManager.RoleExistsAsync(RoleConstants.Administrator);

                    // Check if user is in role via UserManager
                    ViewBag.IsInRoleFromManager = await _userManager.IsInRoleAsync(user, RoleConstants.Administrator);
                }
            }

            var animes = await _context.Animes.ToListAsync();
            ViewBag.StarterPacks = await _context.StarterPacks.Include(s => s.Animes).ToListAsync();
            ViewBag.AnimeTerms = await _context.AnimeTerms.ToListAsync();
            return View(animes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}