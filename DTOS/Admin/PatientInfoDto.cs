namespace MediApp.DTOs;

public class PatientInfoDto
{
    public string MedicationName {get;set;} = string.Empty;
    public int Dose {get;set;}
    public string FullName {get;set;} = string.Empty;
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    public bool IsApproved {get;set;}

}