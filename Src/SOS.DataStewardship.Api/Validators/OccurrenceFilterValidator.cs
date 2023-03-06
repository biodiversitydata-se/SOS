using FluentValidation;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class OccurrenceFilterValidator : AbstractValidator<OccurrenceFilter>
    {
        public OccurrenceFilterValidator()
        {
            RuleFor(m => m.DateFilter).SetValidator(new DateFilterValidator())
                .When(m => m.DateFilter != null);
        }
    }
}