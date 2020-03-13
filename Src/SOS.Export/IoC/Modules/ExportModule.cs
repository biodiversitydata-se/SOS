using Autofac;
using SOS.Export.Factories;
using SOS.Export.Factories.Interfaces;
using SOS.Export.IO.DwcArchive;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Jobs;
using SOS.Export.MongoDb;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

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
            builder.RegisterInstance(Configuration.EmailConfiguration).As<EmailConfiguration>().SingleInstance();
            builder.RegisterInstance(Configuration.FileDestination).As<FileDestination>().SingleInstance();
            
            // Init mongodb
            var exportSettings = Configuration.ProcessedDbConfiguration.GetMongoDbSettings();
            var exportClient = new ExportClient(exportSettings, Configuration.ProcessedDbConfiguration.DatabaseName, Configuration.ProcessedDbConfiguration.BatchSize);
            builder.RegisterInstance(exportClient).As<IExportClient>().SingleInstance();

            // Add factories
            builder.RegisterType<SightingFactory>().As<ISightingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonFactory>().As<ITaxonFactory>().InstancePerLifetimeScope();

            // Repositories mongo
            builder.RegisterType<ProcessedSightingRepository>().As<IProcessedSightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedFieldMappingRepository>().As<IProcessedFieldMappingRepository>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ZendToService>().As<IZendToService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ExportJob>().As<IExportJob>().InstancePerLifetimeScope();

            // DwC Archive
            builder.RegisterType<DwcArchiveFileWriter>().As<IDwcArchiveFileWriter>().SingleInstance();
            builder.RegisterType<DwcArchiveOccurrenceCsvWriter>().As<IDwcArchiveOccurrenceCsvWriter>().SingleInstance();
            builder.RegisterType<ExtendedMeasurementOrFactCsvWriter>().As<IExtendedMeasurementOrFactCsvWriter>().SingleInstance();
        }
    }
}
