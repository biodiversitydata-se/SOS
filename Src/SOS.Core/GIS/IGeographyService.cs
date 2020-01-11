using System;
using System.Collections.Generic;
using System.Text;
using SOS.Core.Models.Observations;

namespace SOS.Core.GIS
{
    public interface IGeographyService
    {
        bool IsObservationInSweden(ProcessedDwcObservation observation);
        void CalculateRegionBelongings(ProcessedDwcObservation observation);
    }
}
