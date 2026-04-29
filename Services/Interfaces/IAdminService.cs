using MediApp.Models;

namespace MediApp.Services;

public interface IAdminService
{
    Task<List<ApplicationUser>> GetPendingDoctors();
}