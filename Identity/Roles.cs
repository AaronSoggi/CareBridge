namespace MediApp.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Patient = "Patient";
    public const string MedicalStaff = $"{Doctor},{Patient}";
}