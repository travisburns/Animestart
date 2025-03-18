using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using animestart.Models;
using System.Threading.Tasks;

namespace animestart.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles if they don't exist
                if (!await roleManager.RoleExistsAsync(RoleConstants.Administrator))
                {
                    await roleManager.CreateAsync(new IdentityRole(RoleConstants.Administrator));
                }
                if (!await roleManager.RoleExistsAsync(RoleConstants.Member))
                {
                    await roleManager.CreateAsync(new IdentityRole(RoleConstants.Member));
                }
            }
        }
    }
}