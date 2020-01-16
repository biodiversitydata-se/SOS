using Autofac;
using SOS.Export.Factories;
using SOS.Export.Factories.Interfaces;
using SOS.Export.IO.DwcArchive;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Jobs;
using SOS.Export.Jobs.Interfaces;
using SOS.Export.MongoDb;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;

namespace SOS.Export.IoC.Modules
{
    /// <summary>
    /// Export module
    /// </summary>
    public class ExportModule : Module
    {
        /// <summary>
        /// Module configuration
        /// </summary>
        public ExportConfiguration Configuration { get; set; }

        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configuration.BlobStorageConfiguration).As<BlobStorageConfiguration>().SingleInstance();
            builder.RegisterInstance(Configuration.FileDestination).As<FileDestination>().SingleInstance();

            // Init mongodb
            var exportSettings = Configuration.MongoDbConfiguration.GetMongoDbSettings();
            var exportClient = new ExportClient(exportSettings, Configuration.MongoDbConfiguration.DatabaseName, Configuration.MongoDbConfiguration.BatchSize);
            builder.RegisterInstance(exportClient).As<IExportClient>().SingleInstance();

            // Add factories
            builder.RegisterType<SightingFactory>().As<ISightingFactory>().InstancePerLifetimeScope();

            // Repositories mongo
            builder.RegisterType<ProcessedSightingRepository>().As<IProcessedSightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ExportDarwinCoreJob>().As<IExportDarwinCoreJob>().InstancePerLifetimeScope();
            builder.RegisterType<DOIJob>().As<IDOIJob>().InstancePerLifetimeScope();

            // DwC Archive
            builder.RegisterType<DwcArchiveFileWriter>().As<IDwcArchiveFileWriter>().SingleInstance();
            builder.RegisterType<DwcArchiveOccurrenceCsvWriter>().As<IDwcArchiveOccurrenceCsvWriter>().SingleInstance();
        }
    }
}
