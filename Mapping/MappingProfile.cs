
using System.Runtime.CompilerServices;
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
        .ForMember(dest => dest.PatientId, opt => opt.Ignore());

        CreateMap<UpdateMedicationDto, Medication>()
        .ForMember(dest => dest.Id, opt => opt.Ignore()).ReverseMap();
        
        CreateMap<MedicationDto, Medication>();

        CreateMap<Medication, MedicationDto>()
        .ForMember(dest => dest.Notes, opt => opt.MapFrom(i => i.Instructions));

        CreateMap<CreateUserDto, ApplicationUser>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(dto => dto.Email));

        CreateMap<Medication, PatientInfoDto>()
        .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(i => (i.Patient.ApplicationUser.FirstName ?? "") + "" + (i.Patient.ApplicationUser.LastName  ?? "")))
        .ForMember(dest => dest.MedicationName, opt => opt.MapFrom(i => i.Name))
        .ForMember(dest => dest.Dose, opt => opt.MapFrom(i => i.Dose))
        .ForMember(dest => dest.EndDate, opt => opt.MapFrom(i => i.EndDate))
        .ForMember(dest => dest.StartDate, opt => opt.MapFrom(i => i.StartDate));


        //Patient
        CreateMap<Patient, PatientDto>()
        .ForMember(dest => dest.Fullname, opt => opt.MapFrom(i => i.ApplicationUser.FirstName + " " + i.ApplicationUser.LastName))
        .ForMember(dest => dest.Doctor, opt => opt.MapFrom(i => i.Doctor.ApplicationUser.FirstName));

        


    }
}