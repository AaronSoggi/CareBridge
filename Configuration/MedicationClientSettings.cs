namespace MediApp.Configuration;

public class MedicationClientSettings
{
    public string BaseUrl {get; set;} = string.Empty;
    public int TimeoutSeconds {get;set;}
}