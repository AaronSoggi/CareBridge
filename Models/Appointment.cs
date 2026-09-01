using MediApp.Enums;

namespace MediApp.Models;

public class Appointment
{
    public int Id {get;set;}
    public DateTime ScheduledAt {get;set;}
    public string Reason {get;set;} = string.Empty;
    public AppointmentStatus Status {get;set;}
    public Patient Patient {get;set;}
    public int PatientId {get;set;}
    public Doctor Doctor {get;set;}
    public int DoctorId {get;set;}

}