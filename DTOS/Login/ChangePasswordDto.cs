using System.ComponentModel.DataAnnotations;

namespace MediApp.DTOs;

public class ChangePasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword {get; set;}
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword {get;set;}
    [Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword")]
    public string ConfirmPassord {get;set;}
}