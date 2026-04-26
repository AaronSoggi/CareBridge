using System.Net;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MediApp.Identity;

public static class AuthorizationPolicies
{
    public static void GeneratePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(Policies.HospitalDoctorsOnly, t => 
        t.RequireClaim("DoctorType", "HospitalDoctor"));

        options.AddPolicy(Policies.VerifiedDoctor, t => 
        t.RequireClaim("IsVerified", "true"));
    } 
}