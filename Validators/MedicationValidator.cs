using System.Data;
using FluentValidation;
using MediApp.DTOs;
using MediApp.Models;

namespace MediApp.Validators;

public class MedicationValidator : AbstractValidator<CreateMedicationDto>
{
    public MedicationValidator()
    {
        RuleFor(w => w.Name).NotEmpty().WithMessage("Medication name field is required")
        .MaximumLength(200);
        RuleFor(r => r.Instructions).NotEmpty().WithMessage("Instructions for medication is required")
        .MaximumLength(500);
        RuleFor(r => r.Dose).GreaterThan(0).WithMessage("Dose must be greater than 0");
        RuleFor(r => r.EndDate).GreaterThan(r => r.StartDate).WithMessage("End date must be greater than the start date.");
    }
}

