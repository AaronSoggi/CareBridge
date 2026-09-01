using System.Runtime.CompilerServices;
using System.Security.Claims;
using Azure.Core;
using MediApp.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace MediApp.Identity;

// creating roles
// creating users
// adding users to specific roles
// for doctors its adding the Isverified claim
public static class IdentitySeeder
{
    public async static Task SeedIdentities(IServiceProvider provider)
    {
        // services that are required for seeding
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        {
            Roles.Admin,
            Roles.Doctor,
            Roles.Patient
        };

        foreach(var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(",", result.Errors.Select(i => i.Description));
                    throw new Exception($"something wentwrong when adding a role: {errors}");
                }
            }
        }

        var patient = await SeedUser(userManager, "Aaron", "kumar", "soggi92@hotmail.co.uk", Roles.Patient);
        var doctor = await SeedUser(userManager, "Aaron", "Soggi", "soggi@hotmail.co.uk", Roles.Doctor);

        var claims = await userManager.GetClaimsAsync(doctor);

        if(!claims.Any(i => i.Type == "IsVerified"))
        {
            var result =  await userManager.AddClaimAsync(doctor, new Claim("IsVerified", "false"));

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(i => i.Description));
                throw new Exception($"Failed to add new claim {errors}");
            }
        }
    }

    public async static Task<ApplicationUser> SeedUser(UserManager<ApplicationUser> userManager, string firstName, string lastName, string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if(user == null)
        {
            user = new ApplicationUser
            {
              FirstName = firstName,
              LastName = lastName,
              Email = email,
              UserName = email,
              Created = DateTime.UtcNow,
              PhoneNumber = "0000"
            };

            var result = await userManager.CreateAsync(user, "Password123!");

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new Exception($"failed to create user {errors}");
            }
        }

        var inRole = await userManager.IsInRoleAsync(user, role);

        if (!inRole)
        {
            var result = await userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new Exception($"unable to assign role{errors}");
            }
        }

        return user;
    }
}