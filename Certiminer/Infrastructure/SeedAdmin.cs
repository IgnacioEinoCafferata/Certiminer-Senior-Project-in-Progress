using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Certiminer.Infrastructure
{
    /// <summary>
    /// Creates the "Admin" role and a first admin user (email/password from config).
    /// Call SeedAdmin.RunAsync(...) once at app startup.
    /// </summary>
    public static class SeedAdmin
    {
        public static async Task RunAsync(IServiceProvider services, IConfiguration config)
        {
            var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

            const string roleName = "Admin";

            // Ensure role exists
            if (!await roleMgr.RoleExistsAsync(roleName))
            {
                var r = await roleMgr.CreateAsync(new IdentityRole(roleName));
                if (!r.Succeeded)
                    throw new Exception("Failed creating Admin role: " +
                        string.Join("; ", r.Errors.Select(e => e.Description)));
            }

            // Read credentials from configuration
            var email = config["Admin:Email"] ?? "admin@certiminer.local";
            var pass = config["Admin:Password"] ?? "ChangeMe!123";

            // Ensure user exists
            var user = await userMgr.FindByEmailAsync(email);
            if (user is null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var created = await userMgr.CreateAsync(user, pass);
                if (!created.Succeeded)
                    throw new Exception("Failed creating Admin user: " +
                        string.Join("; ", created.Errors.Select(e => e.Description)));
            }

            // Ensure user is in Admin role
            if (!await userMgr.IsInRoleAsync(user, roleName))
            {
                var added = await userMgr.AddToRoleAsync(user, roleName);
                if (!added.Succeeded)
                    throw new Exception("Failed adding Admin role to user: " +
                        string.Join("; ", added.Errors.Select(e => e.Description)));
            }
        }
    }
}
