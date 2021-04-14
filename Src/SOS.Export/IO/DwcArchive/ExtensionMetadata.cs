using System.Collections.Generic;

namespace SOS.Export.IO.DwcArchive
{
    public class ExtensionMetadata
    {
        public ExtensionMetadata(string rowType, string fileLocation)
        {
            Fields = new List<ExtensionMetadataField>();
            RowType = rowType;
            FileLocation = fileLocation;
        }

        public string RowType { get; protected set; }
        public string FileLocation { get; protected set; }

        public List<ExtensionMetadataField> Fields { get; set; }

        public class ExtensionMetadataField
        {
            public ExtensionMetadataField(int index, string term, string csvColumnName)
            {
                Index = index;
                Term = term;
                CSVColumnName = csvColumnName;
            }

            public int Index { get; protected set; }
            public string Term { get; protected set; }

            public string CSVColumnName { get; protected set; }
        }

        public static class EmofFactory
        {
            public static ExtensionMetadata Create()
            {
                var extension = new ExtensionMetadata("http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact",
                    "extendedMeasurementOrFact.csv");
                extension.Fields.Add(new ExtensionMetadataField(0, "http://rs.tdwg.org/dwc/terms/occurrenceID",
                    "occurrenceID"));
                extension.Fields.Add(new ExtensionMetadataField(1, "http://rs.tdwg.org/dwc/terms/measurementID",
                    "measurementID"));
                extension.Fields.Add(new ExtensionMetadataField(2, "http://rs.tdwg.org/dwc/terms/measurementType",
                    "measurementType"));
                extension.Fields.Add(new ExtensionMetadataField(3, "http://rs.iobis.org/obis/terms/measurementTypeID",
                    "measurementTypeID"));
                extension.Fields.Add(new ExtensionMetadataField(4, "http://rs.tdwg.org/dwc/terms/measurementValue",
                    "measurementValue"));
                extension.Fields.Add(new ExtensionMetadataField(5, "http://rs.iobis.org/obis/terms/measurementValueID",
                    "measurementValueID"));
                extension.Fields.Add(new ExtensionMetadataField(6, "http://rs.tdwg.org/dwc/terms/measurementAccuracy",
                    "measurementAccuracy"));
                extension.Fields.Add(new ExtensionMetadataField(7, "http://rs.tdwg.org/dwc/terms/measurementUnit",
                    "measurementUnit"));
                extension.Fields.Add(new ExtensionMetadataField(8, "http://rs.iobis.org/obis/terms/measurementUnitID",
                    "measurementUnitID"));
                extension.Fields.Add(new ExtensionMetadataField(9,
                    "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate", "measurementDeterminedDate"));
                extension.Fields.Add(new ExtensionMetadataField(10,
                    "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy", "measurementDeterminedBy"));
                extension.Fields.Add(new ExtensionMetadataField(11, "http://rs.tdwg.org/dwc/terms/measurementRemarks",
                    "measurementRemarks"));
                extension.Fields.Add(new ExtensionMetadataField(12, "http://rs.tdwg.org/dwc/terms/measurementMethod",
                    "measurementMethod"));

                return extension;
            }
        }

        public static class SimpleMultimediaFactory
        {
            public static ExtensionMetadata Create()
            {
                var extension = new ExtensionMetadata("http://rs.gbif.org/terms/1.0/Multimedia",
                    "multimedia.csv");
                extension.Fields.Add(new ExtensionMetadataField(0, "http://rs.tdwg.org/dwc/terms/occurrenceID",
                    "occurrenceID"));
                extension.Fields.Add(new ExtensionMetadataField(1, "http://purl.org/dc/terms/type",
                    "type"));
                extension.Fields.Add(new ExtensionMetadataField(2, "http://purl.org/dc/terms/format",
                    "format"));
                extension.Fields.Add(new ExtensionMetadataField(3, "http://purl.org/dc/terms/identifier",
                    "identifier"));
                extension.Fields.Add(new ExtensionMetadataField(4, "http://purl.org/dc/terms/references",
                    "references"));
                extension.Fields.Add(new ExtensionMetadataField(5, "http://purl.org/dc/terms/title",
                    "title"));
                extension.Fields.Add(new ExtensionMetadataField(6, "http://purl.org/dc/terms/description",
                    "description"));
                extension.Fields.Add(new ExtensionMetadataField(7, "http://purl.org/dc/terms/source",
                    "source"));
                extension.Fields.Add(new ExtensionMetadataField(8, "http://purl.org/dc/terms/audience",
                    "audience"));
                extension.Fields.Add(new ExtensionMetadataField(9, "http://purl.org/dc/terms/created",
                    "created"));
                extension.Fields.Add(new ExtensionMetadataField(10, "http://purl.org/dc/terms/creator",
                    "creator"));
                extension.Fields.Add(new ExtensionMetadataField(11, "http://purl.org/dc/terms/contributor",
                    "contributor"));
                extension.Fields.Add(new ExtensionMetadataField(12, "http://purl.org/dc/terms/publisher",
                    "publisher"));
                extension.Fields.Add(new ExtensionMetadataField(13, "http://purl.org/dc/terms/license",
                    "license"));
                extension.Fields.Add(new ExtensionMetadataField(14, "http://purl.org/dc/terms/rightsHolder",
                    "rightsHolder"));
                return extension;
            }
        }
    }
}