# Field mapping

Field mappings are used for fields that uses a lookup table. A specific field value, for an observation, is stored using the *ProcessedFieldMapValue* class.

```csharp
public class ProcessedFieldMapValue {    
    public int Id { get; set; }       
    public string Value { get; set; }        
}
```

Currently (2020-03-16) the following fields are field mapped:
```csharp
public enum FieldMappingFieldId
{
    Gender = 0,
    Activity = 1,
    County = 2,
    Parish = 3,
    Municipality = 4,
    Province = 5,
    LifeStage = 6,
    Biotope = 7,
    Substrate = 8,
    ValidationStatus = 9,
    Institution = 10,
    Unit = 11,
    BasisOfRecord = 12,
    Continent = 13,
    EstablishmentMeans = 14,
    OccurrenceStatus = 15,
    AccessRights = 16,
    Country = 17,
    Type = 18
}
```

Their values are stored as JSON files in the _Src\SOS.Import\Resources\FieldMappings_ directory.

## Add new field mapping

**Create [Field Name]FieldMapping.json**
1.  Add a new enum value to _FieldMappingFieldId_ for the new field.
2.  Create a new class _[Field Name]FieldMappingFactory_ that implements the _IFieldMappingCreatorFactory_ interface.
3.  Make sure that the _FieldMappingFactory_ class handles the new field in its constructor.
4.  Launch the SOS.Hangfire.UI API and run _FieldMapping/SingleField/Create/[Field Name]_ to generate _[Field Name]FieldMapping.json_

**Import of [Field Name]FieldMapping.json to MongoDb**
1.  Copy _[Field Name]FieldMapping.json_ to the \Src\SOS.Import\Resources\FieldMappings directory. Make sure the Copty to Output Directory property has the _Copy if newer_ value.
2.  Launch the SOS.Hangfire.UI API and run _/HarvestJobs/ImportFieldMapping/Run_. Once the job has been run, the field mapping has entered the FieldMapping collection in the sos-verbatim database.
3.  Then run _/ProcessJob/CopyFieldMapping/Run_ to copy the field mappings from sos-verbatim to sos-processed database.

**Process [Field Name]**
1.  Add a property of type _ProcessedFieldMapValue_ to the _SOS.Lib.Models.Processed.Sighting.ProcessedObservation_ class, for the new field.
2.  Update the process for the data providers that support this field. For Artportalen, the _SOS.Process.Factories.ArtportalenProcessFactory_ class needs to be updated.
3.  Make sure that the _GetFieldMappingsDictionary()_ method delivers the new field mapping.
4.  Update _Sos.Process.Extension.ArtportalenExtensions.ToProcessed()_ with the new field mapping.

**Resolve field mapped values**<br/>
Add the new field name to the following classes when resolving field mapped values:
1. _Sos.Search.Service.Factories.SightingFactory_
2. _SOS.Export.IO.DwcArchive.DwcArchiveOccurrenceCsvWriter_
3. _SOS.Process.Helpers.FieldMappingResolverHelper_