using System;
using System.Linq;
using System.Text;
using Autofac;
using Elasticsearch.Net;
using Nest;
using SOS.Import.DarwinCore;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Import.Factories.FieldMapping;
using SOS.Import.Harvesters;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Managers;
using SOS.Import.Managers.Interfaces;
using SOS.Import.Repositories.Destination;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Destination.ClamPortal;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.FishData;
using SOS.Import.Repositories.Destination.FishData.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Repositories.Destination.Mvm;
using SOS.Import.Repositories.Destination.Mvm.Interfaces;
using SOS.Import.Repositories.Destination.Nors;
using SOS.Import.Repositories.Destination.Nors.Interfaces;
using SOS.Import.Repositories.Destination.Sers;
using SOS.Import.Repositories.Destination.Sers.Interfaces;
using SOS.Import.Repositories.Destination.Shark;
using SOS.Import.Repositories.Destination.Shark.Interfaces;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Repositories.Destination.VirtualHerbarium;
using SOS.Import.Repositories.Destination.VirtualHerbarium.Interfaces;
using SOS.Import.Repositories.Resource;
using SOS.Import.Repositories.Resource.Interfaces;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.IoC.Modules
{
    public class ImportModule : Module
    {
        public (ImportConfiguration ImportConfiguration, 
            MongoDbConfiguration VerbatimDbConfiguration, 
            MongoDbConfiguration ProcessDbConfiguration) Configurations { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            // Add configuration
            if (Configurations.ImportConfiguration.ArtportalenConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ArtportalenConfiguration).As<ArtportalenConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.DwcaConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.DwcaConfiguration).As<DwcaConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.ClamServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ClamServiceConfiguration).As<ClamServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.FishDataServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.FishDataServiceConfiguration).As<FishDataServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.KulServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.KulServiceConfiguration).As<KulServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.MvmServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.MvmServiceConfiguration).As<MvmServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.NorsServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.NorsServiceConfiguration).As<NorsServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.SersServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.SersServiceConfiguration).As<SersServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.SharkServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.SharkServiceConfiguration).As<SharkServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.TaxonAttributeServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.TaxonAttributeServiceConfiguration)
                    .As<TaxonAttributeServiceConfiguration>().SingleInstance();
            if (Configurations.ImportConfiguration.TaxonServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.TaxonServiceConfiguration).As<TaxonServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.VirtualHerbariumServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.VirtualHerbariumServiceConfiguration)
                    .As<VirtualHerbariumServiceConfiguration>().SingleInstance();

            // Vebatim Mongo Db
            var verbatimSettings = Configurations.VerbatimDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IVerbatimClient>(new VerbatimClient(verbatimSettings, Configurations.VerbatimDbConfiguration.DatabaseName,
                Configurations.VerbatimDbConfiguration.ReadBatchSize, Configurations.VerbatimDbConfiguration.WriteBatchSize)).SingleInstance();

            // Processed Mongo Db
            var processedSettings = Configurations.ProcessDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IProcessClient>(new ProcessClient(processedSettings, Configurations.ProcessDbConfiguration.DatabaseName,
                Configurations.ProcessDbConfiguration.ReadBatchSize, Configurations.ProcessDbConfiguration.WriteBatchSize)).SingleInstance();

            // Darwin Core
            builder.RegisterType<DwcArchiveReader>().As<IDwcArchiveReader>().InstancePerLifetimeScope();

            // Managers
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerLifetimeScope();

            // Repositories source
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MetadataRepository>().As<IMetadataRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRepository>().As<ISightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SiteRepository>().As<ISiteRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationRepository>().As<IOrganizationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PersonRepository>().As<IPersonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRelationRepository>().As<ISightingRelationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SpeciesCollectionItemRepository>().As<ISpeciesCollectionItemRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveVerbatimRepository>().As<IDarwinCoreArchiveVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveEventRepository>().As<IDarwinCoreArchiveEventRepository>()
                .InstancePerLifetimeScope();

            // Repositories destination
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingRepository>().As<IFieldMappingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationVerbatimRepository>().As<IFishDataObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationVerbatimRepository>().As<IMvmObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationVerbatimRepository>().As<INorsObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SersObservationVerbatimRepository>().As<ISersObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationVerbatimRepository>().As<ISharkObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SightingVerbatimRepository>().As<ISightingVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<TaxonVerbatimRepository>().As<ITaxonVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationVerbatimRepository>()
                .As<IVirtualHerbariumObservationVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories resource
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().InstancePerLifetimeScope();

            // Add harvesters
            builder.RegisterType<AreaHarvester>().As<IAreaHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenObservationHarvester>().As<IArtportalenObservationHarvester>()
                .SingleInstance();
            builder.RegisterType<ClamPortalObservationHarvester>().As<IClamPortalObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcObservationHarvester>().As<IDwcObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingHarvester>().As<IFieldMappingHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationHarvester>().As<IFishDataObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationHarvester>().As<IKulObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationHarvester>().As<IMvmObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationHarvester>().As<INorsObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationHarvester>().As<ISersObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationHarvester>().As<ISharkObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<TaxonHarvester>().As<ITaxonHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationHarvester>().As<IVirtualHerbariumObservationHarvester>()
                .InstancePerLifetimeScope();

            // Add factories
            builder.RegisterType<ActivityFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<GenderFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LifeStageFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BiotopeFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SubstrateFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ValidationStatusFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<InstitutionFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UnitFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BasisOfRecordFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ContinentFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CountyFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<MunicipalityFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ParishFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProvinceFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TypeFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CountryFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AccessRightsFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OccurrenceStatusFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EstablishmentMeansFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AreaTypeFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DiscoveryMethodFieldMappingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DeterminationMethodFieldMappingFactory>().InstancePerLifetimeScope();

            // Add Services
            builder.RegisterType<ArtportalenDataService>().As<IArtportalenDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationService>().As<IClamObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationService>().As<IFishDataObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationService>().As<IKulObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationService>().As<IMvmObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationService>().As<INorsObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationService>().As<ISersObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationService>().As<ISharkObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonServiceProxy>().As<ITaxonServiceProxy>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonService>().As<ITaxonService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonAttributeService>().As<ITaxonAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationService>().As<IVirtualHerbariumObservationService>()
                .InstancePerLifetimeScope();

            // Service Clients
            builder.RegisterType<MvmService.SpeciesObservationChangeServiceClient>()
                .As<MvmService.ISpeciesObservationChangeService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<AreasHarvestJob>().As<IAreasHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenHarvestJob>().As<IArtportalenHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalHarvestJob>().As<IClamPortalHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveHarvestJob>().As<IDwcArchiveHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingImportJob>().As<IFieldMappingImportJob>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataHarvestJob>().As<IFishDataHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<KulHarvestJob>().As<IKulHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<MvmHarvestJob>().As<IMvmHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<NorsHarvestJob>().As<INorsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<SersHarvestJob>().As<ISersHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<SharkHarvestJob>().As<ISharkHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationsHarvestJob>().As<IObservationsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonHarvestJob>().As<ITaxonHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumHarvestJob>().As<IVirtualHerbariumHarvestJob>()
                .InstancePerLifetimeScope();
        }
    }
}