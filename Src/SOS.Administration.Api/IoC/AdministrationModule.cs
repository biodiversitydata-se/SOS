using Autofac;
using SOS.Administration.Api.Managers;
using SOS.Administration.Api.Managers.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using System.Text;
using SOS.Lib.Configuration.Shared;

namespace SOS.Administration.Api.IoC
{
    /// <summary>
    /// Autofac module
    /// </summary>
    public class AdministrationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Add managers
            builder.RegisterType<IptManager>().As<IIptManager>().InstancePerLifetimeScope();

            // Add services
            builder.RegisterType<FileDownloadService>().As<IFileDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
        }
    }
}