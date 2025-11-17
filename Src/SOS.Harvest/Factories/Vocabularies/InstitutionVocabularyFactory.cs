using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies;

/// <summary>
///     Class for creating institution vocabulary.
/// </summary>
public class InstitutionVocabularyFactory : ArtportalenVocabularyFactoryBase
{
    private readonly IMetadataRepository _artportalenMetadataRepository;
    private readonly ILogger<InstitutionVocabularyFactory> _logger;

    List<string> _institutionCodesThatDoesntExistInArtportalen = new List<string>
        {
             "SLU Swedish Species Information Centre"
            ,"Department of Aquatic Resources (SLU Aqua)"
            ,"Environmental data MVM, SLU"
            ,"Umeå University"
            ,"Swedish Meteorological and Hydrological Institute (SMHI)"
            ,"Swedish Museum of Natural History"
            ,"Station Linné"
            ,"Department of Biology, Lund University"
            ,"Overstellar Solutions AB"
            ,"iNaturalist.org"
            ,"Department of Biology, Lund University"
            ,"Swedish Board of Agriculture"
            ,"Department of Forest Resource Management, SLU"
            ,"Gothenburg Natural History Museum"
            ,"Lund University"
            ,"Biologg"
            ,"NRM"
            ,"MZLU"
            ,"SJV"
            ,"iNaturalist"
            ,"GNM"
            ,"GBIF-SE:GNM"
        };

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="artportalenMetadataRepository"></param>
    /// <param name="logger"></param>
    public InstitutionVocabularyFactory(
        IMetadataRepository artportalenMetadataRepository,
        ILogger<InstitutionVocabularyFactory> logger)
    {
        _artportalenMetadataRepository = artportalenMetadataRepository ??
                                         throw new ArgumentNullException(nameof(artportalenMetadataRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override VocabularyId FieldId => VocabularyId.Institution;
    protected override bool Localized => false;

    protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
    {
        var organizations = await _artportalenMetadataRepository.GetOrganizationsAsync();
        var vocabularyValues = base.ConvertToNonLocalizedVocabularyValues(organizations.ToArray());
        HashSet<string> vocabSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in vocabularyValues) {
            vocabSet.Add(item.Value);
        }            

        int id = 5000;
        foreach (var organization in _institutionCodesThatDoesntExistInArtportalen)
        {
            if (!vocabSet.Contains(organization))
            {
                vocabSet.Add(organization);
                vocabularyValues.Add(CreateNonLocalizedVocabularyValue(++id, organization));
            }
        }

        return vocabularyValues;
    }

    protected override List<ExternalSystemMapping> GetExternalSystemMappings(
        ICollection<VocabularyValueInfo>? vocabularyValues)
    {
        var artportalenVocabularyValues = vocabularyValues!
            .Where(vv => !_institutionCodesThatDoesntExistInArtportalen.Contains(vv.Value))
            .ToList();

        return new List<ExternalSystemMapping>
        {
            GetArtportalenExternalSystemMapping(artportalenVocabularyValues),
            GetDarwinCoreExternalSystemMapping(vocabularyValues)
        };
    }

    private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(
        ICollection<VocabularyValueInfo>? vocabularyValues)
    {
        var externalSystemMapping = new ExternalSystemMapping
        {
            Id = ExternalSystemId.DarwinCore,
            Name = ExternalSystemId.DarwinCore.ToString(),
            Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
            Mappings = new List<ExternalSystemMappingField>()
        };
        var mappingField = new ExternalSystemMappingField
        {
            Key = VocabularyMappingKeyFields.DwcInstitutionCode,
            Description = "The institutionCode term (http://rs.tdwg.org/dwc/terms/institutionCode)"
        };

        if (vocabularyValues?.Any() ?? false)
        {
            var dwcMappingSynonyms = GetDwcMappingSynonyms();
            var dwcMappings = CreateDwcMappings(vocabularyValues, dwcMappingSynonyms);
            mappingField.Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList();
        }

        externalSystemMapping.Mappings.Add(mappingField);
        return externalSystemMapping;
    }

    private Dictionary<string, string> GetDwcMappingSynonyms()
    {
        return new Dictionary<string, string>
        {                
        };
    }
}