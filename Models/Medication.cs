namespace MediApp.Models;

public class Medication
{
    public int Id {get;set;}
    public string? Name {get;set;}
    public int Dose {get; set;}
    public string? Instructions {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    public string UserId {get;set;}
    public ApplicationUser User {get;set;}
}