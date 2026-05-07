using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Infrastructure.Auth
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ── Seed Roles ──
            string[] roles = { "Admin", "Manager", "User", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Log.Information("Role created: {Role}", role);
                }
            }

            // ── Seed Admin User ──
            var adminEmployeeId = "dxadmin";
            var adminEmail = "admin@aseldev.com";
            var adminPassword = "Aselp@ssw0rd27";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmployeeId,
                    EmployeeId = adminEmployeeId,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    Department = "IT",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Log.Information("Admin user seeded: {Email}", adminEmail);
                }
                else
                {
                    Log.Error("Admin seed failed: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                adminUser.EmployeeId = adminEmployeeId;
                adminUser.UserName = adminEmployeeId;
                adminUser.NormalizedUserName = userManager.NormalizeName(adminEmployeeId);
                adminUser.IsActive = true;
                adminUser.EmailConfirmed = true;

                var updateResult = await userManager.UpdateAsync(adminUser);
                if (updateResult.Succeeded)
                {
                    Log.Information("Admin user login updated to employee id: {EmployeeId}", adminEmployeeId);
                }
                else
                {
                    Log.Error("Admin login update failed: {Errors}",
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }

                if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var passwordResult = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);

                    if (passwordResult.Succeeded)
                    {
                        Log.Information("Admin password reset to configured seed password.");
                    }
                    else
                    {
                        Log.Error("Admin password reset failed: {Errors}",
                            string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                    }
                }

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
