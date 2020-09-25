using Autofac;
using SOS.Export.IO.DwcArchive;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Jobs;
using SOS.Export.Managers;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.IoC.Modules
{
    /// <summary>
    ///     Export module
    /// </summary>
    public class ExportModule : Module
    {
        /// <summary>
        ///     Module configuration
        /// </summary>
        public (ExportConfiguration ExportConfiguration, MongoDbConfiguration ProcessDbConfiguration, BlobStorageConfiguration BlobStorageConfiguration) Configurations { get; set; }

        /// <summary>
        ///     Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configurations.BlobStorageConfiguration).As<BlobStorageConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.DwcaFilesCreationConfiguration).As<DwcaFilesCreationConfiguration>().SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.FileDestination).As<FileDestination>().SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.ZendToConfiguration).As<ZendToConfiguration>().SingleInstance();

            // Processed Mongo Db
            var processedSettings = Configurations.ProcessDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IProcessClient>(new ProcessClient(processedSettings, Configurations.ProcessDbConfiguration.DatabaseName,
                Configurations.ProcessDbConfiguration.ReadBatchSize, Configurations.ProcessDbConfiguration.WriteBatchSize)).SingleInstance();

            // Add managers
            builder.RegisterType<ObservationManager>().As<IObservationManager>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonManager>().As<ITaxonManager>().InstancePerLifetimeScope();
            builder.RegisterType<FilterManager>().As<IFilterManager>().InstancePerLifetimeScope();

            // Repositories mongo
            builder.RegisterType<DOIRepository>().As<IDOIRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<Lib.Repositories.Processed.ProcessedFieldMappingRepository>().As<Lib.Repositories.Processed.Interfaces.IProcessedFieldMappingRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().SingleInstance();

            // Services
            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ZendToService>().As<IZendToService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ExportAndSendJob>().As<IExportAndSendJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportAndStoreJob>().As<IExportAndStoreJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportToDoiJob>().As<IExportToDoiJob>().InstancePerLifetimeScope();
            builder.RegisterType<UploadToStoreJob>().As<IUploadToStoreJob>().InstancePerLifetimeScope();

            // DwC Archive
            builder.RegisterType<DwcArchiveFileWriterCoordinator>().As<IDwcArchiveFileWriterCoordinator>().SingleInstance();
            builder.RegisterType<DwcArchiveFileWriter>().As<IDwcArchiveFileWriter>().SingleInstance();
            builder.RegisterType<DwcArchiveOccurrenceCsvWriter>().As<IDwcArchiveOccurrenceCsvWriter>().SingleInstance();
            builder.RegisterType<ExtendedMeasurementOrFactCsvWriter>().As<IExtendedMeasurementOrFactCsvWriter>()
                .SingleInstance();

            // Helpers
            builder.RegisterType<FieldMappingResolverHelper>().As<IFieldMappingResolverHelper>().SingleInstance();
        }
    }
}