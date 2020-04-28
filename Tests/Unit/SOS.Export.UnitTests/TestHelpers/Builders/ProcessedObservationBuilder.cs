using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.UnitTests.TestHelpers.Builders
{
    public class ProcessedObservationBuilder : BuilderBase<ProcessedObservationBuilder, ProcessedObservation>
    {
        public ProcessedObservationBuilder WithDecimalLatitude(double decimalLatitude)
        {
            return With(entity => entity.Location.DecimalLatitude = decimalLatitude);
        }

        public ProcessedObservationBuilder WithDecimalLongitude(double decimalLongitude)
        {
            return With(entity => entity.Location.DecimalLongitude = decimalLongitude);
        }

        public ProcessedObservationBuilder WithCoordinateUncertaintyInMeters(int? coordinateUncertaintyInMeters)
        {
            return With(entity => entity.Location.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters);
        }

        public ProcessedObservationBuilder WithOccurrenceRemarks(string occurrenceRemarks)
        {
            return With(entity => entity.Occurrence.OccurrenceRemarks = occurrenceRemarks);
        }

        protected override ProcessedObservation CreateEntity()
        {
            var observation = new ProcessedObservation
            {
                Location = new ProcessedLocation(),
                Event = new ProcessedEvent(),
                Identification = new ProcessedIdentification(),
                MaterialSample = new ProcessedMaterialSample(),
                Occurrence = new ProcessedOccurrence(),
                Taxon = new ProcessedTaxon()
            };

            return observation;
        }
    }
}