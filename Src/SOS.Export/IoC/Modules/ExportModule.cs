using Autofac;
using SOS.Export.IO.DwcArchive;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Jobs;
using SOS.Export.Managers;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
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
        public (ExportConfiguration ExportConfiguration, 
            MongoDbConfiguration ProcessDbConfiguration, 
            BlobStorageConfiguration BlobStorageConfiguration,
            DataCiteServiceConfiguration DataCiteServiceConfiguration) Configurations { get; set; }

        /// <summary>
        ///     Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configurations.BlobStorageConfiguration).As<BlobStorageConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.DataCiteServiceConfiguration).As<DataCiteServiceConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.DOIConfiguration).As<DOIConfiguration>()
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
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingRepository>().As<IFieldMappingRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<DataCiteService>().As<IDataCiteService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<ZendToService>().As<IZendToService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<CreateDoiJob>().As<ICreateDoiJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportAndSendJob>().As<IExportAndSendJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportAndStoreJob>().As<IExportAndStoreJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportToDoiJob>().As<IExportToDoiJob>().InstancePerLifetimeScope();
            builder.RegisterType<UploadToStoreJob>().As<IUploadToStoreJob>().InstancePerLifetimeScope();

            // DwC Archive
            builder.RegisterType<DwcArchiveFileWriterCoordinator>().As<IDwcArchiveFileWriterCoordinator>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveFileWriter>().As<IDwcArchiveFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveOccurrenceCsvWriter>().As<IDwcArchiveOccurrenceCsvWriter>().InstancePerLifetimeScope();
            builder.RegisterType<ExtendedMeasurementOrFactCsvWriter>().As<IExtendedMeasurementOrFactCsvWriter>()
                .InstancePerLifetimeScope();

            // Helpers, static data => single instance 
            builder.RegisterType<FieldMappingResolverHelper>().As<IFieldMappingResolverHelper>().SingleInstance();
        }
    }
}