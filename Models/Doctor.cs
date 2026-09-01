namespace MediApp.Models;

public class Doctor
{
    public int Id {get;set;}
    public string Specialisation {get;set;} = string.Empty;
    public string ApplicationUserId {get;set;}
    public ApplicationUser ApplicationUser {get;set;}
    public ICollection<Patient> Patients { get;set;} = new List<Patient>();
}