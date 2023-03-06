using FluentValidation;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class EventsFilterValidator : AbstractValidator<EventsFilter>
    {
        public EventsFilterValidator()
        {            
            RuleFor(m => m.DateFilter).SetValidator(new DateFilterValidator())
                .When(m => m.DateFilter != null);
        }
    }
}