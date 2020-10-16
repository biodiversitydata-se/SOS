using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.UnitTests.TestHelpers.Builders
{
    public class ProcessedObservationBuilder : BuilderBase<ProcessedObservationBuilder, Observation>
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

        protected override Observation CreateEntity()
        {
            var observation = new Observation
            {
                Location = new Location(),
                Event = new Event(),
                Identification = new Identification(),
                MaterialSample = new MaterialSample(),
                Occurrence = new Occurrence(),
                Taxon = new Taxon()
            };

            return observation;
        }
    }
}