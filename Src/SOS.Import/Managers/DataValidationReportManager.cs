using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Factories.Validation;
using SOS.Import.Factories.Validation.Interfaces;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Managers
{
    public class DataValidationReportManager : IDataValidationReportManager
    {
        private readonly Dictionary<DataProviderType, IDataValidationReportFactory> _reportCreatorByType;

        public DataValidationReportManager(
            KulDataValidationReportFactory kulDataValidationReportFactory,
            SersDataValidationReportFactory sersDataValidationReportFactory,
            NorsDataValidationReportFactory norsDataValidationReportFactory, 
            VirtualHerbariumValidationReportFactory virtualHerbariumDataValidationReportFactory)
        {
            if (kulDataValidationReportFactory == null) throw new ArgumentNullException(nameof(kulDataValidationReportFactory));
            if (sersDataValidationReportFactory == null) throw new ArgumentNullException(nameof(sersDataValidationReportFactory));
            if (norsDataValidationReportFactory == null) throw new ArgumentNullException(nameof(norsDataValidationReportFactory));
            if (virtualHerbariumDataValidationReportFactory == null) throw new ArgumentNullException(nameof(virtualHerbariumDataValidationReportFactory));

            _reportCreatorByType = new Dictionary<DataProviderType, IDataValidationReportFactory>
            {
                {DataProviderType.KULObservations, kulDataValidationReportFactory},
                {DataProviderType.SersObservations, sersDataValidationReportFactory},
                {DataProviderType.NorsObservations, norsDataValidationReportFactory},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumDataValidationReportFactory}
            };
        }

        public async Task<DataValidationReport<object, Observation>> CreateDataValidationReport(
            DataProvider dataProvider, 
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 10, 
            int nrInvalidObservationsInReport = 100)
        {
            _reportCreatorByType.TryGetValue(dataProvider.Type, out var reportCreator);
            if (reportCreator == null) throw new ArgumentException($"CreateValidationReport() - No support for data provider: {dataProvider}");

            var dataValidationSummary = await reportCreator.CreateDataValidationSummary(
                dataProvider,
                maxNrObservationsToRead,
                nrValidObservationsInReport,
                nrInvalidObservationsInReport);

            return dataValidationSummary;
        }
    }
}