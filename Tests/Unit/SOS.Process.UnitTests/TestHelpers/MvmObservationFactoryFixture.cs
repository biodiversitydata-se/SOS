using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.Mvm;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Process.UnitTests.TestHelpers;

/// <summary>
/// Loads and transforms reference data once. Each test receives its own factory,
/// repositories, caches and copies of the mutable reference data.
/// </summary>
public sealed class MvmObservationFactoryFixture
{
    private readonly Dictionary<int, Taxon> _taxa;
    private readonly List<Vocabulary> _vocabularies;
    private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyMappings;
    private readonly List<AreaWithGeometry> _areas;

    public MvmObservationFactoryFixture()
    {
        _taxa = MessagePackHelper.CreateListFromMessagePackFile<Taxon>("Resources/MammaliaProcessedTaxa.msgpck")
            .ToDictionary(taxon => taxon.Id);
        _vocabularies = MessagePackHelper.CreateListFromMessagePackFile<Vocabulary>("Resources/Vocabularies.msgpck");
        _vocabularyMappings = VocabularyHelper.GetVocabulariesDictionary(ExternalSystemId.DarwinCore, _vocabularies, true);
        _areas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province }).ToList();
    }

    public MvmObservationFactory CreateFactory(ValidationStatusId? verificationStatusId = null)
    {
        var vocabularies = _vocabularies.Clone();
        var vocabularyRepository = new Mock<IVocabularyRepository>();
        vocabularyRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(vocabularies);
        var mappings = _vocabularyMappings.ToDictionary(
            mapping => mapping.Key,
            mapping => (IDictionary<object, int>)new Dictionary<object, int>(mapping.Value));
        var areas = _areas.Select(area => new AreaWithGeometry(area.AreaType, area.FeatureId)
        {
            Name = area.Name,
            Geometry = area.Geometry.Copy()
        }).ToList();
        var areaRepository = ProcessedAreaRepositoryStubFactory.Create(areas);
        var configuration = new ProcessConfiguration();
        return new MvmObservationFactory(
            new DataProvider { DefaultVerificationStatus = verificationStatusId },
            _taxa.Clone(),
            mappings,
            new AreaHelper(new AreaConfiguration(), areaRepository.Object),
            new ProcessTimeManager(configuration),
            vocabularyRepository.Object,
            configuration);
    }
}
