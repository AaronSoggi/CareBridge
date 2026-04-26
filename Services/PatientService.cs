using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediApp.Data;
using MediApp.DTOs;
using MediApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _Dbcontext;
    private readonly IMapper _mapper;
    public PatientService(ApplicationDbContext Dbcontext, IMapper mapper)
    {
        _Dbcontext = Dbcontext;
        _mapper = mapper;
    }

    public async Task<List<PatientInfoDto>> GetPatientInfo()
    {
        // as this is readonly information we should just use projectTo
        return await _Dbcontext.Medications
        .Where(i => i.EndDate > DateTime.UtcNow)
        .OrderBy(i => i.Name)
        .ProjectTo<PatientInfoDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

    }

    // Get all users that havent been approved and their account was created over a year ago
    public async Task<List<ApplicationUser>> GetUnapprovedUsers()
    {
        return await _Dbcontext.Users
        .Include(i => i.Medications)
        .Include(i => i.Profile)
            .Where(i => i.Profile != null 
            && !i.Profile.IsApproved 
            && i.Created > DateTime.Now.AddYears(-1))
            .ToListAsync();
    }

    public async Task<List<Medication>> GetMedicationsHighDose()
    {
        return await _Dbcontext.Medications
        .Where(i => i.Dose > 200 
        && i.EndDate > DateTime.UtcNow.AddYears(1))
        .OrderBy(i => i.Name).ToListAsync();
    }
}