using Castle.Components.DictionaryAdapter;

namespace MediApp.DTOs;

public class PatientDto
{
    public string Fullname {get; set;} = string.Empty;
    public string NhsNumber {get;set;} = string.Empty;
    public DateTime DateOfBirth {get;set;}
    public string Doctor {get;set;} = string.Empty;
}