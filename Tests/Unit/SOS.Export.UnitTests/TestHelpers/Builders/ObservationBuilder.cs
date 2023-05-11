using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.UnitTests.TestHelpers.Builders
{
    public class ObservationBuilder : BuilderBase<ObservationBuilder, Observation>
    {
        public ObservationBuilder WithOccurrenceId(string occurrenceId)
        {
            return With(entity => entity.Occurrence.OccurrenceId = occurrenceId);
        }
        public ObservationBuilder WithScientificName(string scientificName)
        {
            return With(entity => entity.Taxon.ScientificName = scientificName);
        }

        public ObservationBuilder WithDecimalLatitude(double decimalLatitude)
        {
            return With(entity => entity.Location.DecimalLatitude = decimalLatitude);
        }

        public ObservationBuilder WithDecimalLongitude(double decimalLongitude)
        {
            return With(entity => entity.Location.DecimalLongitude = decimalLongitude);
        }

        public ObservationBuilder WithCoordinateUncertaintyInMeters(int? coordinateUncertaintyInMeters)
        {
            return With(entity => entity.Location.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters);
        }

        public ObservationBuilder WithOccurrenceRemarks(string occurrenceRemarks)
        {
            return With(entity => entity.Occurrence.OccurrenceRemarks = occurrenceRemarks);
        }

        protected override Observation CreateEntity()
        {
            var observation = new Observation
            {
                Location = new Location(LocationType.Point),
                Event = new Event(System.DateTime.Now.AddHours(-2), System.DateTime.Now.AddHours(-1)),
                Identification = new Identification(),
                MaterialSample = new MaterialSample(),
                Occurrence = new Occurrence(),
                Taxon = new Taxon()
            };

            return observation;
        }
    }
}