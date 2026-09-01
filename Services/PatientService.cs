using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediApp.Data;
using MediApp.DTOs;
using MediApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediApp.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _Dbcontext;
    private readonly IMapper _mapper;

    private readonly IMemoryCache _cache;
    public PatientService(ApplicationDbContext Dbcontext, IMapper mapper, IMemoryCache cache)
    {
        _Dbcontext = Dbcontext;
        _mapper = mapper;
        _cache = cache;

    }

    public async Task<List<PatientDto>> GetPatientsAsync(int pageNumber, int pageSize)
    {
        var cacheKey = $"patientList:PageNumber:{pageNumber}:PageSize{pageSize}";

        if(_cache.TryGetValue(cacheKey, out List<PatientDto>? patients))
        {
            return patients;
        }
        

        var dto = await _Dbcontext.Patients
        .OrderBy(i => i.ApplicationUser.FirstName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

        _cache.Set(cacheKey, dto, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        return dto;
        
    }
}