namespace MediApp.Models;

public class Patient
{
    public int Id {get;set;}
    public DateTime DateOfBirth {get;set;}
    public string NhsNumber {get;set;} = string.Empty;

    public string ApplicationUserId {get;set;}
    public ApplicationUser ApplicationUser {get;set;}

    public int DoctorId {get;set;}
    public Doctor Doctor {get;set;}

    public ICollection<Appointment> Appointments {get;set;} = new List<Appointment>();
    public ICollection<Medication> Medications {get;set;} = new List<Medication>();
}