using MediApp.DTOs;
using MediApp.Models;

namespace MediApp.Services;

public interface IMedicationService
{
    Task<ServiceResult> CreateMedicationAsync(CreateMedicationDto dto, string userId);
    Task<ServiceResult> UpdateMedicationAsync(UpdateMedicationDto dto, string userId);
    Task<UpdateMedicationDto?> GetUpdateMedicationAsync(string userId, int medicationId);
    Task<PagedResult<MedicationDto>> GetMedicationsAsync(string userId, int pageNumber, int pageSize)
}