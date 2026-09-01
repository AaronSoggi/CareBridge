using MediApp.Models;
using MediApp.DTOs;
namespace MediApp.Services;


public interface IPatientService
{
    Task<List<PatientDto>> GetPatientsAsync(int pageNumber, int pageSize = 10);
}