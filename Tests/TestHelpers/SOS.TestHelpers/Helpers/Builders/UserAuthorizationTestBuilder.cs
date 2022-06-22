using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.UserService;

namespace SOS.TestHelpers.Helpers.Builders
{
    public class UserAuthorizationTestBuilder : BuilderBase<UserAuthorizationTestBuilder, AuthorityModel>
    {
        public UserAuthorizationTestBuilder WithAreaAccess(AreaType areaType, string featureId)
        {
            return With(entity => entity.Areas.Add(new AreaModel {AreaTypeId = (int)areaType, FeatureId = featureId}));
        }

        public UserAuthorizationTestBuilder WithAreaAccess(AreaModel area)
        {
            return With(entity => entity.Areas.Add(area));
        }
        
        public UserAuthorizationTestBuilder WithAuthorityIdentity(string authorityIdentity)
        {
            return With(entity => entity.AuthorityIdentity = authorityIdentity);
        }

        public UserAuthorizationTestBuilder WithMaxProtectionLevel(int maxProtectionLevel)
        {
            return With(entity => entity.MaxProtectionLevel = maxProtectionLevel);
        }

        public UserAuthorizationTestBuilder WithTaxonIdsAccess(ICollection<int> taxonIds)
        {
            return With(entity => entity.TaxonIds = taxonIds);
        }

        public UserAuthorizationTestBuilder WithTaxonIdsAccess(int taxonId)
        {
            return With(entity => entity.TaxonIds = new List<int> { taxonId });
        }

        protected override AuthorityModel CreateEntity()
        {
            var authorityModel = new AuthorityModel();
            authorityModel.Areas = new List<AreaModel>();
            return authorityModel;
        }
    }
}