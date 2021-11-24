using System.Linq;
using System.Threading.Tasks;
using Application.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seed
{
    public class SeedAdminUser
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole<long>> roleManager)
        {
            var adminUser = new User
            {
                UserName = "adminuser",
                Email = "adminuser@gmail.com",
                FirstName = "admin",
                LastName = "user"
            };

            if (userManager.Users.All(u => u.Id != adminUser.Id))
            {
                var user = await userManager.FindByEmailAsync(adminUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(adminUser, "P@ssw0rd");
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin.ToString());
                }
            }

            var testUser = new User
            {
                UserName = "test",
                Email = "test@gmail.com",
                FirstName = "test",
                LastName = "user"
            };

            if (userManager.Users.All(u => u.Id != testUser.Id))
            {
                var user = await userManager.FindByEmailAsync(testUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(testUser, "P@ssw0rd");
                    await userManager.AddToRoleAsync(testUser, UserRoles.Admin.ToString());
                    await userManager.AddToRoleAsync(testUser, UserRoles.Manager.ToString());
                }
            }
        }
    }
}