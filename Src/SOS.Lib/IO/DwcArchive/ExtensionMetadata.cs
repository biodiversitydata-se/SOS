﻿using SOS.Export.Models;
using System.Collections.Generic;

namespace SOS.Lib.IO.DwcArchive
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
            public ExtensionMetadataField(int index, string term, string csvColumnName, bool excludeMetaRow = false)
            {
                Index = index;
                Term = term;
                CSVColumnName = csvColumnName;
                ExcludeMetaRow = excludeMetaRow;
            }

            public int Index { get; protected set; }
            public string Term { get; protected set; }
            public string CSVColumnName { get; protected set; }

            /// <summary>
            /// Indicates whether this column shouldn't be written to meta.xml.
            /// </summary>
            public bool ExcludeMetaRow { get; protected set; }
        }

        public static class EmofFactory
        {
            public static ExtensionMetadata Create(bool isEventCore = false)
            {
                var index = 0;
                var extension = new ExtensionMetadata("http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact",
                    "extendedMeasurementOrFact.txt");
                if (isEventCore)
                {
                    extension.Fields.Add(new ExtensionMetadataField(index++, "", "eventID", true));
                }

                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/occurrenceID", "occurrenceID"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementID", "measurementID"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementType", "measurementType"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.iobis.org/obis/terms/measurementTypeID", "measurementTypeID"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementValue", "measurementValue"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.iobis.org/obis/terms/measurementValueID", "measurementValueID"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementAccuracy", "measurementAccuracy"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementUnit", "measurementUnit"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.iobis.org/obis/terms/measurementUnitID", "measurementUnitID"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate", "measurementDeterminedDate"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy", "measurementDeterminedBy"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementRemarks", "measurementRemarks"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://rs.tdwg.org/dwc/terms/measurementMethod", "measurementMethod"));

                return extension;
            }
        }

        public static class SimpleMultimediaFactory
        {
            public static ExtensionMetadata Create(bool isEventCore = false)
            {
                var index = 0;
                var extension = new ExtensionMetadata("http://rs.gbif.org/terms/1.0/Multimedia", "multimedia.txt");

                if (isEventCore)
                {
                    extension.Fields.Add(new ExtensionMetadataField(index++, "", "eventID", true));
                }
                extension.Fields.Add(new ExtensionMetadataField(index++, "", "occurrenceID", true));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/type", "type"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/format", "format"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/identifier", "identifier"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/references", "references"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/title", "title"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/description", "description"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/source", "source"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/audience", "audience"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/created", "created"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/creator", "creator"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/contributor", "contributor"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/publisher", "publisher"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/license", "license"));
                extension.Fields.Add(new ExtensionMetadataField(index++, "http://purl.org/dc/terms/rightsHolder", "rightsHolder"));

                return extension;
            }
        }

        public static class OccurrenceFactory
        {
            public static ExtensionMetadata Create(IEnumerable<FieldDescription> fieldDescriptions)
            {
                var extension = new ExtensionMetadata("http://rs.tdwg.org/dwc/terms/Occurrence",
                    "occurrence.txt");
                int index = 0;
                foreach (var fieldDescription in fieldDescriptions)
                {
                    extension.Fields.Add(new ExtensionMetadataField(index, fieldDescription.DwcIdentifier, fieldDescription.Name));
                    index++;
                }

                return extension;
            }
        }
    }
}