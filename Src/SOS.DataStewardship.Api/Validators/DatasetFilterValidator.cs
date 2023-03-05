using FluentValidation;
using FluentValidation.Results;
using MimeKit.Cryptography;
using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Validators
{
    public class DatasetFilterValidator : AbstractValidator<DatasetFilter>
    {
        public DatasetFilterValidator()
        {            
            //RuleFor(m => m.DateFilter).SetValidator(new DateFilterValidator());
            RuleFor(m => m.DateFilter).SetValidator(new DateFilterValidator())
                .When(m => m.DateFilter != null);
        }        
    }
}