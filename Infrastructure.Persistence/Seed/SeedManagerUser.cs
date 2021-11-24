using System.Linq;
using System.Threading.Tasks;
using Application.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seed
{
    public class SeedManagerUser
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole<long>> roleManager)
        {
            var managerUser = new User
            {
                UserName = "manageruser",
                Email = "manageruser@gmail.com",
                FirstName = "manager",
                LastName = "user"
            };

            if (userManager.Users.All(u => u.Id != managerUser.Id))
            {
                var user = await userManager.FindByEmailAsync(managerUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(managerUser, "P@ssw0rd");
                    await userManager.AddToRoleAsync(managerUser, UserRoles.Manager.ToString());
                }
            }
        }
    }
}