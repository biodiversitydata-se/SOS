//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Text;
//using SOS.Lib.Models.Verbatim.DarwinCore;

//namespace SOS.Export.UnitTests.TestHelpers.Builders
//{
//    public class DwcObservationVerbatimBuilder : BuilderBase<DwcObservationVerbatimBuilder, DwcObservationVerbatim>
//    {
//        public DwcObservationVerbatimBuilder WithDecimalLatitude(double decimalLatitude)
//        {
//            return With(entity => entity.DecimalLatitude = decimalLatitude.ToString(CultureInfo.InvariantCulture));
//        }

//        public DwcObservationVerbatimBuilder WithDecimalLatitude(string decimalLatitude)
//        {
//            return With(entity => entity.DecimalLatitude = decimalLatitude);
//        }

//        public DwcObservationVerbatimBuilder WithDecimalLongitude(double decimalLongitude)
//        {
//            return With(entity => entity.DecimalLongitude = decimalLongitude.ToString(CultureInfo.InvariantCulture));
//        }

//        public DwcObservationVerbatimBuilder WithDecimalLongitude(string decimalLongitude)
//        {
//            return With(entity => entity.DecimalLongitude = decimalLongitude);
//        }

//        public DwcObservationVerbatimBuilder WithGeodeticDatum(string geodaticDatum)
//        {
//            return With(entity => entity.GeodeticDatum = geodaticDatum);
//        }

//        public DwcObservationVerbatimBuilder WithCoordinateUncertaintyInMeters(int? coordinateUncertaintyInMeters)
//        {
//            return With(entity => entity.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters?.ToString());
//        }

//        public DwcObservationVerbatimBuilder WithCoordinateUncertaintyInMeters(string coordinateUncertaintyInMeters)
//        {
//            return With(entity => entity.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters);
//        }

//        public DwcObservationVerbatimBuilder WithOccurrenceRemarks(string occurrenceRemarks)
//        {
//            return With(entity => entity.OccurrenceRemarks = occurrenceRemarks);
//        }

//        protected override DwcObservationVerbatim CreateEntity()
//        {
//            var observation = new DwcObservationVerbatim();
//            return observation;
//        }
//    }
//}

