﻿using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Managers.Interfaces
{
    public interface IDataValidationReportManager
    {
        public Task<DataValidationReport<object, Observation>> CreateDataValidationReport(
            DataProvider dataProvider,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 10,
            int nrInvalidObservationsInReport = 100);
    }
}