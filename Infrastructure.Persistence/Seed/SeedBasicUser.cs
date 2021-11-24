using System.Linq;
using System.Threading.Tasks;
using Application.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seed
{
    public class SeedBasicUser
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole<long>> roleManager)
        {
            var basicUser = new User
            {
                UserName = "basicuser",
                Email = "basicuser@gmail.com",
                FirstName = "basic",
                LastName = "user"
            };

            if (userManager.Users.All(u => u.Id != basicUser.Id))
            {
                var user = await userManager.FindByEmailAsync(basicUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(basicUser, "P@ssw0rd");
                    await userManager.AddToRoleAsync(basicUser, UserRoles.Basic.ToString());
                }
            }
        }

    }
}