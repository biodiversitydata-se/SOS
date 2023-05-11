using FluentValidation;

namespace SOS.DataStewardship.Api.Validators
{
    public class GeographicsFilterValidator : AbstractValidator<Contracts.Models.GeographicsFilter>
    {
        public GeographicsFilterValidator()
        {
            RuleFor(m => m.Geometry).SetValidator(new GeometryFilterValidator())
                .When(m => m.Geometry != null);
        }
    }
}