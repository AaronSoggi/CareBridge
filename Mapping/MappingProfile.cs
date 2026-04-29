
using AutoMapper;
using MediApp.DTOs;
using MediApp.Models;

namespace MediApp.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateMedicationDto, Medication>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<UpdateMedicationDto, Medication>()
        .ForMember(dest => dest.Id, opt => opt.Ignore()).ReverseMap();
        
        CreateMap<MedicationDto, Medication>();

        CreateMap<Medication, MedicationDto>()
        .ForMember(dest => dest.Notes, opt => opt.MapFrom(i => i.Instructions));

        CreateMap<CreateUserDto, ApplicationUser>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(dto => dto.Email));

        CreateMap<Medication, PatientInfoDto>()
        .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(i => (i.User.FirstName ?? "") + "" + (i.User.LastName ?? "")))
        .ForMember(dest => dest.MedicationName, opt => opt.MapFrom(i => i.Name))
        .ForMember(dest => dest.Dose, opt => opt.MapFrom(i => i.Dose))
        .ForMember(dest => dest.EndDate, opt => opt.MapFrom(i => i.EndDate))
        .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(i => i.User.Profile.IsApproved))
        .ForMember(dest => dest.StartDate, opt => opt.MapFrom(i => i.StartDate));

        


    }
}