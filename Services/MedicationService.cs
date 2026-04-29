using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediApp.Data;
using MediApp.DTOs;
using MediApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediApp.Services;

public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MedicationService> _logger;
    private readonly IMapper _mapper;

    private readonly IMemoryCache _cache;
    public MedicationService(ApplicationDbContext dbContext, ILogger<MedicationService> logger,
    IMapper mapper, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ServiceResult> CreateMedicationAsync(CreateMedicationDto dto, string userId)
    {
        try
        {
            var existingMedication = await _dbContext.Medications.FirstOrDefaultAsync(t => t.Name == dto.Name && t.UserId == userId);

            if(existingMedication != null)
            {
                _logger.LogWarning($"Error occured, this medication already exists!: {dto.Name}");
                return ServiceResult.Fail("Error occured, this medication already exists!");
            }

            var medication = _mapper.Map<Medication>(dto);
            medication.UserId = userId;

            await _dbContext.Medications.AddAsync(medication);
            await _dbContext.SaveChangesAsync();

            return ServiceResult.Ok("Medication has succesfully been created");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "something went wrong, failed to create medication");
            return ServiceResult.Fail("Unable to create medication");
        }
        
    }

    public async Task<ServiceResult> UpdateMedicationAsync(UpdateMedicationDto dto, string userId)
    {
        try
        {
            var medication = await _dbContext.Medications.FirstOrDefaultAsync(t => t.Id == dto.Id && t.UserId == userId);

            if(medication == null)
            {
                _logger.LogWarning($"Unable to update medication: {medication}");
                return ServiceResult.Missing("something went wrong when trying to Update the medication");
            }

            _mapper.Map(dto,medication);
            await _dbContext.SaveChangesAsync();

            _cache.Remove($"medication:{dto.Id}");
            _cache.Remove($"medications:user{userId}");

            return ServiceResult.Ok("Medication has been updated succesfully");
        }
        catch
        {
            _logger.LogError($"something went wrong when attempting to update the medication: {dto.Id}");
            return ServiceResult.Fail("request to update medication did not go through sucessfully.");
        }
        
    }

    public async Task<UpdateMedicationDto?> GetUpdateMedicationAsync(string userId, int medicationId)
    {

        var medication = await _dbContext.Medications.FirstOrDefaultAsync(i => i.Id == medicationId && i.UserId == userId);

        if(medication == null)
        {
            _logger.LogWarning("This medication cannot be found: {userid} Id: {medicationId}", userId, medicationId);
            return null;
        }

        var updateMedicationDto = _mapper.Map<UpdateMedicationDto>(medication);

        return updateMedicationDto;
    }

    public async Task<PagedResult<MedicationDto>> GetMedicationsAsync(string userId, int pageNumber, int pageSize)
    {
        // var cacheKey = $"medications:user:{userId}:page:{pageNumber}:size:{pageSize}";

        // if(_cache.TryGetValue(cacheKey, out PagedResult<MedicationDto>? medications))
        // {
        //     return medications;
        // }

        var query = _dbContext.Medications
        .Where(i => i.UserId == userId)
        .OrderBy(i => i.Name);

        var count = await query.CountAsync();

        var medicationDtos = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<MedicationDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

        var result = new PagedResult<MedicationDto>
        {
            Items = medicationDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItemCount = count
        };

        // _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        // {
        //     SlidingExpiration = TimeSpan.FromMinutes(5),
        //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        // });

        return result;

    }
}