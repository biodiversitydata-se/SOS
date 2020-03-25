﻿using Autofac;
using SOS.Import.DarwinCore;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Jobs.Import;
using SOS.Import.Factories;
using SOS.Import.Factories.FieldMapping;
using SOS.Import.Harvesters;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
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
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.IoC.Modules
{
    public class ImportModule : Module
    {
        public ImportConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            if (Configuration.ClamServiceConfiguration != null)
                builder.RegisterInstance(Configuration.ClamServiceConfiguration).As<ClamServiceConfiguration>().SingleInstance();
            if (Configuration.KulServiceConfiguration != null)
                builder.RegisterInstance(Configuration.KulServiceConfiguration).As<KulServiceConfiguration>().SingleInstance();
            if (Configuration.ArtportalenConfiguration != null)
                builder.RegisterInstance(Configuration.ArtportalenConfiguration).As<ArtportalenConfiguration>().SingleInstance();
            if (Configuration.TaxonAttributeServiceConfiguration != null)
                builder.RegisterInstance(Configuration.TaxonAttributeServiceConfiguration).As<TaxonAttributeServiceConfiguration>().SingleInstance();
            if (Configuration.TaxonServiceConfiguration != null)
                builder.RegisterInstance(Configuration.TaxonServiceConfiguration).As<TaxonServiceConfiguration>().SingleInstance();

            // Init mongodb
            if (Configuration.VerbatimDbConfiguration != null)
            {
                var importSettings = Configuration.VerbatimDbConfiguration.GetMongoDbSettings();
                var importClient = new ImportClient(
                    importSettings, 
                    Configuration.VerbatimDbConfiguration.DatabaseName,
                    Configuration.VerbatimDbConfiguration.BatchSize);
                builder.RegisterInstance(importClient).As<IImportClient>().SingleInstance();
            }

            // Darwin Core
            builder.RegisterType<DwcArchiveReader>().As<IDwcArchiveReader>().InstancePerLifetimeScope();

            // Repositories source
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MetadataRepository>().As<IMetadataRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRepository>().As<ISightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SiteRepository>().As<ISiteRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationRepository>().As<IOrganizationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PersonRepository>().As<IPersonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRelationRepository>().As<ISightingRelationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesCollectionItemRepository>().As<ISpeciesCollectionItemRepository>().InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveVerbatimRepository>().As<IDarwinCoreArchiveVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories destination
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingVerbatimRepository>().As<ISightingVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonVerbatimRepository>().As<ITaxonVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingRepository>().As<IFieldMappingRepository>().InstancePerLifetimeScope();

            // Add harvesters
            builder.RegisterType<ClamPortalObservationHarvester>().As<IClamPortalObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<AreaHarvester>().As<IAreaHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationHarvester>().As<IKulObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenObservationHarvester>().As<IArtportalenObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonHarvester>().As<ITaxonHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingHarvester>().As<IFieldMappingHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<DwcObservationHarvester>().As<IDwcObservationHarvester>().InstancePerLifetimeScope();

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

            // Add Services
            builder.RegisterType<ArtportalenDataService>().As<IArtportalenDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationService>().As<IClamObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonServiceProxy>().As<ITaxonServiceProxy>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonService>().As<ITaxonService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonAttributeService>().As<ITaxonAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationService>().As<IKulObservationService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ArtportalenHarvestJob>().As<IArtportalenHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalHarvestJob>().As<IClamPortalHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingImportJob>().As<IFieldMappingImportJob>().InstancePerLifetimeScope();
            builder.RegisterType<GeoAreasHarvestJob>().As<IGeoAreasHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<KulHarvestJob>().As<IKulHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationsHarvestJob>().As<IObservationsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonHarvestJob>().As<ITaxonHarvestJob>().InstancePerLifetimeScope();
        }
    }
}
