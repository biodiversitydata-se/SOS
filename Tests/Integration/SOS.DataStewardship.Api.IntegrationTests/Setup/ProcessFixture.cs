using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup
{
    internal class ProcessFixture
    {
        private IProcessClient _processClient;
        private IAreaHelper _areaHelper;
        private DwcaObservationFactory _darwinCoreFactory;
        private DwcaEventFactory _dwcaEventFactory;
        private DwcaDatasetFactory _dwcaDatasetFactory;
        private IVocabularyRepository _vocabularyRepository;
        private Dictionary<int, Taxon> _taxaById;
        private ITaxonRepository _taxonRepository;
        private IProcessTimeManager _processTimeManager;
        private ProcessConfiguration _processConfiguration;
        
        private DataProvider _testDataProvider = new DataProvider { Id = 1, Identifier = "TestDataProvider" };
        private List<Taxon> _taxa;

        public ProcessFixture(IAreaHelper areaHelper,
            IProcessClient processClient,
            IVocabularyRepository vocabularyRepository,
            ITaxonRepository taxonRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration
            ) 
        {            
            _areaHelper = areaHelper;
            _processClient = processClient;
            _vocabularyRepository = vocabularyRepository;
            _taxonRepository = taxonRepository;
            _processTimeManager = processTimeManager;
            _processConfiguration = processConfiguration;

            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            _taxa = await _taxonRepository.GetAllAsync();
            _taxaById = _taxa.ToDictionary(m => m.Id, m => m);
        }

        public DwcaObservationFactory GetDwcaObservationFactory(bool initAreaHelper)
        {
            if (initAreaHelper) InitAreaHelper();
            if (_darwinCoreFactory == null)
            {                                
                _darwinCoreFactory = CreateDwcaObservationFactoryAsync(_testDataProvider).Result;
            }            

            return _darwinCoreFactory;
        }

        public DwcaEventFactory GetDwcaEventFactory(bool initAreaHelper)
        {
            if (initAreaHelper) InitAreaHelper();
            if (_dwcaEventFactory == null)
            {
                _dwcaEventFactory = CreateDwcaEventFactoryAsync(_testDataProvider).Result;
            }
                
            return _dwcaEventFactory;
        }

        public DwcaDatasetFactory GetDwcaDatasetFactory()
        {
            if (_dwcaDatasetFactory == null)
            {
                _dwcaDatasetFactory = new DwcaDatasetFactory(_testDataProvider, _processTimeManager, _processConfiguration);
            }

            return _dwcaDatasetFactory;
        }

        private void InitAreaHelper()
        {
            if (!_areaHelper.IsInitialized)
            {
                _areaHelper.InitializeAsync().Wait();
            }
        }

        private async Task<DwcaEventFactory> CreateDwcaEventFactoryAsync(DataProvider dataProvider)
        {
            var factory = await DwcaEventFactory.CreateAsync(
                dataProvider,
                _vocabularyRepository,
                _areaHelper,
                _processTimeManager,
                _processConfiguration);

            return factory;
        }

        private async Task<DwcaObservationFactory> CreateDwcaObservationFactoryAsync(DataProvider dataProvider)
        {
            var factory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                _taxaById,
                _vocabularyRepository,
                _areaHelper,
                _processTimeManager,
                _processConfiguration);

            return factory;
        }
    }
}