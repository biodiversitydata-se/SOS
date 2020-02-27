using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Export.UnitTests.TestHelpers.Builders
{
    public class ProcessedSightingBuilder : BuilderBase<ProcessedSightingBuilder, ProcessedSighting>
    {
        public static ProcessedSightingBuilder InMemory => new ProcessedSightingBuilder();

        public ProcessedSightingBuilder()
        {
        }

        public ProcessedSightingBuilder WithDecimalLatitude(double decimalLatitude)
        {
            return With(entity => entity.Location.DecimalLatitude = decimalLatitude);
        }

        public ProcessedSightingBuilder WithDecimalLongitude(double decimalLongitude)
        {
            return With(entity => entity.Location.DecimalLongitude = decimalLongitude);
        }

        public ProcessedSightingBuilder WithCoordinateUncertaintyInMeters(int? coordinateUncertaintyInMeters)
        {
            return With(entity => entity.Location.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters);
        }

        public ProcessedSightingBuilder WithOccurrenceRemarks(string occurrenceRemarks)
        {
            return With(entity => entity.Occurrence.Remarks = occurrenceRemarks);
        }

        protected override ProcessedSighting CreateEntity()
        {
            var sighting = new ProcessedSighting
            {
                Location = new ProcessedLocation(),
                Event = new ProcessedEvent(),
                Identification = new ProcessedIdentification(),
                MaterialSample = new ProcessedMaterialSample(),
                Occurrence = new ProcessedOccurrence(),
                Taxon = new ProcessedTaxon()
            };

            return sighting;
        }
    }
}