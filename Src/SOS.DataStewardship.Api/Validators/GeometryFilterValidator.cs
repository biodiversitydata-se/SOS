using FluentValidation;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class GeometryFilterValidator : AbstractValidator<GeometryFilter>
    {
        public GeometryFilterValidator()
        {
            RuleFor(m => m.MaxDistanceFromGeometries).GreaterThan(0)
                .WithMessage("MaxDistanceFromGeometries must be a value larger than 0, when specified")
                .When(m => m.MaxDistanceFromGeometries.HasValue);
        }
    }
}