using Microsoft.AspNetCore.Identity;

namespace MediApp.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName {get;set;}
    public string? LastName {get;set;}
    public DateTime Created {get;set;}
    public ICollection<Medication> Medications {get;set;} = new List<Medication>();
    public UserProfile? Profile {get;set;}
}