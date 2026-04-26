namespace MediApp.DTOs;

public class CreateMedicationDto()
{
    public string? Name {get;set;}
    public int Dose {get; set;}
    public string? Instructions {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}

}