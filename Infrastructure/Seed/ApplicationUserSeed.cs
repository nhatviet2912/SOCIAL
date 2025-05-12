using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seed;

public static class ApplicationUserSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roleName = "Admin";
        var userEmail = "nhatviet2912@gmail.com";
        var userPassword = "123456";
        var userName = "nhatviet";

        // 1. Tạo role nếu chưa có
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }

        // 2. Tạo user nếu chưa có
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = userEmail,
                Member = new Member()
                {
                    Id = Guid.NewGuid(),
                    Name = userName
                }
            };

            var result = await userManager.CreateAsync(user, userPassword);
            if (!result.Succeeded)
            {
                throw new Exception();
            }
        }

        // 3. Gán user vào role nếu chưa có
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
    }
}