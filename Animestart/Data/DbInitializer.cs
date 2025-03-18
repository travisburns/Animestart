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
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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