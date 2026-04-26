using System.ComponentModel.DataAnnotations;
using Humanizer;

namespace MediApp.Models;

public class UserProfile
{
    public int Id {get;set;}
    public string? FullName {get;set;}
    public string? ImageUrl {get;set;}
    public string? Biography {get;set;}
    [Required]
    public bool IsApproved {get;set;}
    public string UserId {get;set;} 
    public ApplicationUser User {get;set;}

}