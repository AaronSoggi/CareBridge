namespace MediApp.DTOs;

public class MedicationDto()
{
    public int Id {get;set;}
    public string Name {get;set;} = string.Empty;
    public int Dose {get; set;}
    public string Notes {get;set;} = string.Empty;
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
}