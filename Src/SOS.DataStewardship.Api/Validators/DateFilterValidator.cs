using FluentValidation;

namespace SOS.DataStewardship.Api.Validators
{
    public class DateFilterValidator : AbstractValidator<Contracts.Models.DateFilter>
    {
        public DateFilterValidator()
        {
            RuleFor(m => m.EndDate)
                .GreaterThanOrEqualTo(r => r.StartDate)
                .When(m => m.StartDate.HasValue && m.EndDate.HasValue)
                .WithMessage("EndDate must be after StartDate");
        }
    }
}