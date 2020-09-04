using System.Collections.Generic;

namespace SOS.Lib.Models.DataValidation
{
    public class DwcaValidationRemarksBuilder
    {
        public int NrValidatedObservations { get; set; }
        public int NrMissingCoordinateUncertaintyInMeters { get; set; }
        public int NrMissingIdentificationVerificationStatus { get; set; }

        public List<string> CreateRemarks()
        {
            List<string> remarks = new List<string>();
            if (NrValidatedObservations == NrMissingIdentificationVerificationStatus)
            {
                remarks.Add("IdentificationVerificationStatus values are missing. Observation.Identification.Validated will be set to false for each observation. Consider to provide the identificationVerificationStatus term in the DwC-A file. Example values: verified, unverified");
            }

            if (NrValidatedObservations == NrMissingCoordinateUncertaintyInMeters)
            {
                remarks.Add("CoordinateUncertaintyInMeters values are missing. Observation.Location.CoordinateUncertaintyInMeters will be set to default value 10 000m for each observation. Consider to provide the coordinateUncertaintyInMeters term in the DwC-A file. Example values: 1, 25, 100, 1000. 0 is not a valid value for this term.");
            }
            return remarks;
        }
    }
}