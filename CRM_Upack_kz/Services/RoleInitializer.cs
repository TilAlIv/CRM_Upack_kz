using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CRM_Upack_kz.Models;

namespace WebStudio.Services
{
    public class RoleInitializer
    {
        public static async Task Initializer(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            string adminEmail = "admin@admin.com";
            string adminPassword = "Q1w2e3r4t%";
            string avatarPath = $"/Images/Avatars/defaultavatar.jpg";

            string userEmail = "user@user.com";
            string userPassword = "12345Aa";
            
            string merchEmail = "merch@merch.com";
            string merchPassword = "12345Aa";
            
            var roles = new[] {"admin", "user", "merch"};

            foreach (var role in roles)
            {
                if (await roleManager.FindByNameAsync(role) is null)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                User admin = new User
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    Name = "admin",
                    Surname = "admin",
                    AvatarPath = avatarPath,
                    RoleDisplay = "admin",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }

            if (await userManager.FindByEmailAsync(userEmail) is null)
            {
                User user = new User
                {
                    Email = userEmail,
                    UserName = userEmail,
                    Name = "user",
                    Surname = "user",
                    AvatarPath = avatarPath,
                    RoleDisplay = "user",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };
                var userResult = await userManager.CreateAsync(user, userPassword);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
                }
            }

            if (await userManager.FindByEmailAsync(merchEmail) is null)
            {
                User merch = new User
                {
                    Email = merchEmail,
                    UserName = merchEmail,
                    Name = "merch",
                    Surname = "merch",
                    AvatarPath = avatarPath,
                    RoleDisplay = "merch",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };
                var userResult = await userManager.CreateAsync(merch, userPassword);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(merch, "merch");
                }
            }
        }
    }
}