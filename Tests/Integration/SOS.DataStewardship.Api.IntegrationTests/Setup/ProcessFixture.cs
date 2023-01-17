using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
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
        private IVocabularyRepository _vocabularyRepository;
        private Dictionary<int, Taxon> _taxaById;
        private ITaxonRepository _taxonRepository;
        private IProcessTimeManager _processTimeManager;
        private ProcessConfiguration _processConfiguration;

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

            //bool useTaxonZipCollection = false; // GetUseTaxonZipCollection();
            //if (useTaxonZipCollection)
            //{
            //    _taxa = GetTaxaFromZipFile();
            //}
            //else
            //{
            //    _taxa = await _taxonRepository.GetAllAsync();
            //}
            _taxa = await _taxonRepository.GetAllAsync();
            _taxaById = _taxa.ToDictionary(m => m.Id, m => m);
        }

        public DwcaObservationFactory GetDarwinCoreFactory(bool initAreaHelper)
        {
            if (_darwinCoreFactory == null)
            {
                var dataProvider = new DataProvider() { Id = 1, Identifier = "Artportalen" };
                _areaHelper = new AreaHelper(new AreaRepository(_processClient, new NullLogger<AreaRepository>()));
                _darwinCoreFactory = CreateDarwinCoreFactoryAsync(dataProvider).Result;
            }

            if (initAreaHelper && !_areaHelper.IsInitialized)
            {
                _areaHelper.InitializeAsync().Wait();
            }

            return _darwinCoreFactory;
        }


        private async Task<DwcaObservationFactory> CreateDarwinCoreFactoryAsync(DataProvider dataProvider)
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
