namespace MediApp.Services;

public interface IDoctorService
{
    Task<ServiceResult> VerifyDoctorAsync(string userId);
}