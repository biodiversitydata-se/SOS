using Autofac;
using SOS.Lib.Configuration.Shared;
using SOS.Observations.Api.Database;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.IoC.Modules
{
    /// <summary>
    /// Export module
    /// </summary>
    public class SearchModule : Module
    {
        public MongoDbConfiguration MongoDbConfiguration { get; set; }

        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Processed Mongo Db
            var processedSettings = MongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, MongoDbConfiguration.DatabaseName, MongoDbConfiguration.BatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();

            // Add factories
            builder.RegisterType<ObservationManager>().As<IObservationManager>().SingleInstance(); // InstancePerLifetimeScope?
            builder.RegisterType<ProcessInfoManager>().As<IProcessInfoManager>().SingleInstance(); // InstancePerLifetimeScope
            builder.RegisterType<TaxonManager>().As<ITaxonManager>().SingleInstance();
            builder.RegisterType<FieldMappingManager>().As<IFieldMappingManager>().SingleInstance();

            // Repositories mongo
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>().SingleInstance();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().SingleInstance();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().SingleInstance();
            builder.RegisterType<ProcessedFieldMappingRepository>().As<IProcessedFieldMappingRepository>().SingleInstance();
        }
    }
}
