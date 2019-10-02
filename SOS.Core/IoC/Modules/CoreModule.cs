using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using SOS.Core.GIS;
using SOS.Core.Repositories;
using SOS.Core.Services;

namespace SOS.Core.IoC.Modules
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DoiRepository>()
                .As<IDoiRepository>();
            builder.RegisterType<DoiService>()
                .As<IDoiService>();
            builder.RegisterType<FileBasedGeographyServiceEx>()
                .As<IGeographyService>().SingleInstance();


            //builder.RegisterType<SpeciesObservationService.Core.Implementation.Taxonomy.TaxonManager>()
            //    .As<SpeciesObservationService.Core.Implementation.Taxonomy.ITaxonManager>(); // Multiple instances of Taxon manager
            //builder.RegisterType<SpeciesObservationService.Core.Implementation.Taxonomy.FileBasedTaxonDataRepository>()
            //    .As<SpeciesObservationService.Core.Implementation.Taxonomy.ITaxonDataRepository>().SingleInstance();

            ////Process repository
            //ProcessRepositoryMongoDB.Init();
            //builder.RegisterType<ProcessRepositoryMongoDB>().As<IProcessRepository>();

            ////Observation repository
            //ObservationRepositoryMongoDB.Init();
            //builder.RegisterType<ObservationRepositoryMongoDB>().As<IObservationRepository>();

        }
    }
}
