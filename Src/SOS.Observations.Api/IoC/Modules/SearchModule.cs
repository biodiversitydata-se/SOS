using Autofac;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Services;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Api.IoC.Modules
{
    /// <summary>
    ///     Export module
    /// </summary>
    public class SearchModule : Module
    {
        public ObservationApiConfiguration ObservationApiConfiguration { get; set; }

        /// <summary>
        ///     Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Processed Mongo Db
            var processedDbConfiguration = ObservationApiConfiguration.ProcessDbConfiguration;
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, 
                processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize,
                processedDbConfiguration.WriteBatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();

            // Add configuration
            builder.RegisterInstance(ObservationApiConfiguration.BlobStorageConfiguration)
                .As<BlobStorageConfiguration>().SingleInstance();

            // Add managers
            builder.RegisterType<AreaManager>().As<IAreaManager>().SingleInstance(); // InstancePerLifetimeScope
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().SingleInstance();
            builder.RegisterType<DOIManager>().As<IDOIManager>().SingleInstance();
            builder.RegisterType<FieldMappingManager>().As<IFieldMappingManager>().SingleInstance();
            builder.RegisterType<ObservationManager>().As<IObservationManager>()
                .SingleInstance(); // InstancePerLifetimeScope?
            builder.RegisterType<ProcessInfoManager>().As<IProcessInfoManager>()
                .SingleInstance(); // InstancePerLifetimeScope
            builder.RegisterType<TaxonManager>().As<ITaxonManager>().SingleInstance();
            builder.RegisterType<FilterManager>().As<IFilterManager>().SingleInstance();

            // Add repositories
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().SingleInstance();
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().SingleInstance();
            builder.RegisterType<DOIRepository>().As<IDOIRepository>().SingleInstance();
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .SingleInstance();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().SingleInstance();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().SingleInstance();
            builder.RegisterType<ProcessedFieldMappingRepository>().As<IProcessedFieldMappingRepository>()
                .SingleInstance();

            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().SingleInstance();
        }
    }
}