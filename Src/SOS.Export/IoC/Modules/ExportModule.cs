﻿using Autofac;
using Autofac.Core;
using SOS.Export.Jobs;
using SOS.Export.Managers;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
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
            CryptoConfiguration CryptoConfiguration,
            DataCiteServiceConfiguration DataCiteServiceConfiguration,
            UserServiceConfiguration UserServiceConfiguration,
            AreaConfiguration AreaConfiguration) Configurations
        { get; set; }

        /// <summary>
        ///     Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configurations.BlobStorageConfiguration).As<BlobStorageConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.CryptoConfiguration).As<CryptoConfiguration>()
               .SingleInstance();
            builder.RegisterInstance(Configurations.DataCiteServiceConfiguration).As<DataCiteServiceConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.DOIConfiguration).As<DOIConfiguration>()
                .SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.DwcaFilesCreationConfiguration).As<DwcaFilesCreationConfiguration>().SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.FileDestination).As<FileDestination>().SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.ZendToConfiguration).As<ZendToConfiguration>().SingleInstance();
            builder.RegisterInstance(Configurations.ExportConfiguration.VocabularyConfiguration).As<VocabularyConfiguration>().SingleInstance();
            builder.RegisterInstance(Configurations.UserServiceConfiguration).As<UserServiceConfiguration>().SingleInstance();
            builder.RegisterInstance(Configurations.AreaConfiguration).As<AreaConfiguration>().SingleInstance();
            
            // Processed Mongo Db
            var processedSettings = Configurations.ProcessDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IProcessClient>(new ProcessClient(processedSettings, Configurations.ProcessDbConfiguration.DatabaseName,
                Configurations.ProcessDbConfiguration.ReadBatchSize, Configurations.ProcessDbConfiguration.WriteBatchSize)).SingleInstance();

            // Add security
            builder.RegisterType<CurrentUserAuthorization>().As<IAuthorizationProvider>().InstancePerLifetimeScope();

            // Add cache
            builder.RegisterType<AreaCache>().As<IAreaCache>().SingleInstance();
            builder.RegisterType<ProcessedConfigurationCache>().As<ICache<string, ProcessedConfiguration>>().SingleInstance();
            builder.RegisterType<ProjectCache>().As<ICache<int, ProjectInfo>>().SingleInstance();

            // Add managers
            builder.RegisterType<FilterManager>().As<IFilterManager>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationManager>().As<IObservationManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectManager>().As<IProjectManager>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonManager>().As<ITaxonManager>().SingleInstance();
           
            // Repositories elastic
            builder.RegisterType<ProcessedObservationCoreRepository>().As<IProcessedObservationCoreRepository>()
                .InstancePerLifetimeScope();

            // Repositories mongo
            builder.RegisterType<TaxonRepository>().As<ITaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedConfigurationRepository>().As<IProcessedConfigurationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<VocabularyRepository>().As<IVocabularyRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<UserExportRepository>().As<IUserExportRepository>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<CryptoService>().As<ICryptoService>().InstancePerLifetimeScope();
            builder.RegisterType<DataCiteService>().As<IDataCiteService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
            builder.RegisterType<ZendToService>().As<IZendToService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<CreateDoiJob>().As<ICreateDoiJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportAndSendJob>().As<IExportAndSendJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportAndStoreJob>().As<IExportAndStoreJob>().InstancePerLifetimeScope();
            builder.RegisterType<ExportToDoiJob>().As<IExportToDoiJob>().InstancePerLifetimeScope();
            builder.RegisterType<UploadToStoreJob>().As<IUploadToStoreJob>().InstancePerLifetimeScope();

            // DwC Archive
            builder.RegisterType<DwcArchiveFileWriterCoordinator>().As<IDwcArchiveFileWriterCoordinator>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveFileWriter>().As<IDwcArchiveFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveEventCsvWriter>().As<IDwcArchiveEventCsvWriter>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveEventFileWriter>().As<IDwcArchiveEventFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveOccurrenceCsvWriter>().As<IDwcArchiveOccurrenceCsvWriter>().InstancePerLifetimeScope();
            builder.RegisterType<ExtendedMeasurementOrFactCsvWriter>().As<IExtendedMeasurementOrFactCsvWriter>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SimpleMultimediaCsvWriter>().As<ISimpleMultimediaCsvWriter>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CsvFileWriter>().As<ICsvFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<ExcelFileWriter>().As<IExcelFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<GeoJsonFileWriter>().As<IGeoJsonFileWriter>().InstancePerLifetimeScope();
            builder.RegisterType<AnalysisManager>().As<IAnalysisManager>().InstancePerLifetimeScope();
            builder.RegisterType<GeneralizationResolver>().As<IGeneralizationResolver>().SingleInstance();

            // Helpers, static data => single instance 
            builder.RegisterType<VocabularyValueResolver>().As<IVocabularyValueResolver>().SingleInstance();            
        }
    }
}