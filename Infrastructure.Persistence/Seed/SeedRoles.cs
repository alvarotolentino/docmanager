using System.Threading.Tasks;
using Application.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seed
{
    public class SeedRoles
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole<long>> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.Manager.ToString()));
            await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.Basic.ToString()));
        }
    }
}