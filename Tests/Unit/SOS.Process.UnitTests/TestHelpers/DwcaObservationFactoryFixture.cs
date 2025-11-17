using SOS.Harvest.Managers;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SOS.Process.UnitTests.TestHelpers;

/// <summary>
///     A fixture that creates an instance of DwcaObservationFactory with the following properties:
///     - Only Mammalia taxa are used.
///     - Only County and Province regions are used.
///     - All vocabulary mappings are used.
///     All data is loaded from files in the Resources folder.
/// </summary>
public class DwcaObservationFactoryFixture : IDisposable
{
    public DwcaObservationFactoryFixture()
    {
        DwcaObservationFactory = CreateDwcaObservationFactory();
    }

    public DwcaObservationFactory DwcaObservationFactory { get; private set; }

    public void Dispose()
    {
        DwcaObservationFactory = null;
    }

    public DwcaObservationFactory CreateDwcaObservationFactory(ValidationStatusId? verificationStatusId = null)
    {
        var dataProviderDummy = new DataProvider { DefaultVerificationStatus = verificationStatusId };
        var mammaliaTaxa =
            MessagePackHelper.CreateListFromMessagePackFile<Taxon>(
                @"Resources/MammaliaProcessedTaxa.msgpck");
        var mammaliaTaxonByTaxonId = mammaliaTaxa.ToDictionary(t => t.Id, t => t);            
        var processedAreaRepositoryStub =
            ProcessedAreaRepositoryStubFactory.Create(AreaType.County, AreaType.Province);
        var vocabularyRepository = VocabularyRepositoryStubFactory.Create();
        var vocabularies = vocabularyRepository.Object.GetAllAsync().Result;          
        var dwcaVocabularyById = VocabularyHelper.GetVocabulariesDictionary(
            ExternalSystemId.DarwinCore,
            vocabularies.ToArray(),
            true);

        var areaHelper = new AreaHelper(new AreaConfiguration(), processedAreaRepositoryStub.Object);
        var processConfiguration = new ProcessConfiguration();
        var factory = DwcaObservationFactory.CreateAsync(
            dataProviderDummy,
            mammaliaTaxonByTaxonId,
            dwcaVocabularyById,
            areaHelper,
            new ProcessTimeManager(processConfiguration),
            processConfiguration).Result;
        return factory;
    }
}

[CollectionDefinition("DwcaObservationFactory collection")]
public class DwcaObservationFactoryCollection : ICollectionFixture<DwcaObservationFactoryFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}