﻿using System.Text;
using Autofac;
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
using SOS.Import.MongoDb;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Destination.ClamPortal;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
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
using SOS.Lib.Jobs.Import;

namespace SOS.Import.IoC.Modules
{
    public class ImportModule : Module
    {
        public ImportConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Add configuration
            if (Configuration.ArtportalenConfiguration != null)
                builder.RegisterInstance(Configuration.ArtportalenConfiguration).As<ArtportalenConfiguration>()
                    .SingleInstance();
            if (Configuration.ClamServiceConfiguration != null)
                builder.RegisterInstance(Configuration.ClamServiceConfiguration).As<ClamServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.KulServiceConfiguration != null)
                builder.RegisterInstance(Configuration.KulServiceConfiguration).As<KulServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.MvmServiceConfiguration != null)
                builder.RegisterInstance(Configuration.MvmServiceConfiguration).As<MvmServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.NorsServiceConfiguration != null)
                builder.RegisterInstance(Configuration.NorsServiceConfiguration).As<NorsServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.SersServiceConfiguration != null)
                builder.RegisterInstance(Configuration.SersServiceConfiguration).As<SersServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.SharkServiceConfiguration != null)
                builder.RegisterInstance(Configuration.SharkServiceConfiguration).As<SharkServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.TaxonAttributeServiceConfiguration != null)
                builder.RegisterInstance(Configuration.TaxonAttributeServiceConfiguration)
                    .As<TaxonAttributeServiceConfiguration>().SingleInstance();
            if (Configuration.TaxonServiceConfiguration != null)
                builder.RegisterInstance(Configuration.TaxonServiceConfiguration).As<TaxonServiceConfiguration>()
                    .SingleInstance();
            if (Configuration.VirtualHerbariumServiceConfiguration != null)
                builder.RegisterInstance(Configuration.VirtualHerbariumServiceConfiguration)
                    .As<VirtualHerbariumServiceConfiguration>().SingleInstance();

            // Init verbatim mongodb
            if (Configuration.VerbatimDbConfiguration != null)
            {
                var importSettings = Configuration.VerbatimDbConfiguration.GetMongoDbSettings();
                var importClient = new ImportClient(
                    importSettings,
                    Configuration.VerbatimDbConfiguration.DatabaseName,
                    Configuration.VerbatimDbConfiguration.BatchSize);
                builder.RegisterInstance(importClient).As<IImportClient>().SingleInstance();
            }

            // Init resource mongodb
            if (Configuration.ResourceDbConfiguration != null)
            {
                var resourceDbSettings = Configuration.ResourceDbConfiguration.GetMongoDbSettings();
                var resourceDbClient = new ResourceDbClient(
                    resourceDbSettings,
                    Configuration.ResourceDbConfiguration.DatabaseName,
                    Configuration.ResourceDbConfiguration.BatchSize);
                builder.RegisterInstance(resourceDbClient).As<IResourceDbClient>().SingleInstance();
            }

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
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingRepository>().As<IFieldMappingRepository>().InstancePerLifetimeScope();
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


            // Add harvesters
            builder.RegisterType<AreaHarvester>().As<IAreaHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenObservationHarvester>().As<IArtportalenObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalObservationHarvester>().As<IClamPortalObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcObservationHarvester>().As<IDwcObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingHarvester>().As<IFieldMappingHarvester>().InstancePerLifetimeScope();
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
            builder.RegisterType<ArtportalenHarvestJob>().As<IArtportalenHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalHarvestJob>().As<IClamPortalHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveHarvestJob>().As<IDwcArchiveHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingImportJob>().As<IFieldMappingImportJob>().InstancePerLifetimeScope();
            builder.RegisterType<AreasHarvestJob>().As<IAreasHarvestJob>().InstancePerLifetimeScope();
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