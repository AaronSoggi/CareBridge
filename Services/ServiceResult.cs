namespace MediApp.Services;

public class ServiceResult
{
    public bool Success {get;set;}
    public bool NotFound {get;set;}
    public string Message {get;set;} = string.Empty;

    public static ServiceResult Ok(string message) => new()
    {
        Success = true,
        Message = message
    };

    public static ServiceResult Missing(string message) => new()
    {
        Success = false,
        NotFound = true,
        Message = message  
    };

    public static ServiceResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}