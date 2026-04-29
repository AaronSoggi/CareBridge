using MediApp.Data;
using MediApp.Identity;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Services;


public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        
    }

    public async Task<List<ApplicationUser>> GetPendingDoctors() 
    {
        var pendingDoctors = new List<ApplicationUser>();

        var users = await _dbContext.Users.ToListAsync();

        foreach(var doctor in users)
        {
            var result = await _userManager.GetClaimsAsync(doctor);

            if(result != null)
            {
                var isPending = result.FirstOrDefault(i => i.Type == "IsVerified" && i.Value == "false");

                if(await _userManager.IsInRoleAsync(doctor, Roles.Doctor) && isPending != null)
                {
                    pendingDoctors.Add(doctor);
                }
            }
            
        }
        return pendingDoctors;
    }

}