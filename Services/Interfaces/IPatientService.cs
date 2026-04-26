using MediApp.Models;
using MediApp.DTOs;
namespace MediApp.Services;


public interface IPatientService
{
    Task<List<PatientInfoDto>> GetPatientInfo();
}