namespace MediApp.DTOs;

public class UpdateMedicationDto()
{
    public int Id {get;set;}
    public string Name {get;set;}
    public int Dose {get; set;}
    public string Instructions {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
}