# Vocabularies

Vocabularies are used for fields that uses a lookup table. A specific field value, for an observation, is stored using the *VocabularyValue* class.

```csharp
public class VocabularyValue {    
    public int Id { get; set; }       
    public string Value { get; set; }        
}
```

Currently (2020-12-16) the following fields have vocabularies:
```csharp
public enum VocabularyId
{
    Gender = 0,
    Activity = 1,
    LifeStage = 6,
    Biotope = 7,
    Substrate = 8,
    VerificationStatus = 9,
    Institution = 10,
    Unit = 11,
    BasisOfRecord = 12,
    Continent = 13,
    EstablishmentMeans = 14,
    OccurrenceStatus = 15,
    AccessRights = 16,
    Country = 17,
    Type = 18,
    AreaType = 19,
    DiscoveryMethod = 20,
    DeterminationMethod = 21,
    ReproductiveCondition = 22,
    Behavior = 23
}
```

Their values are stored as JSON files in the _Src\SOS.Import\Resources\Vocabularies_ directory.

## Add new vocabulary

**Create [Vocabulary Name]Vocabulary.json**
1.  Add a new enum value to _VocabularyId_ for the new field.
2.  Create a new class _[Vocabulary Name]VocabularyFactory_ that inherits either _ArtportalenVocabularyFactoryBase_ or _DwcVocabularyFactoryBase_.
3.  Make sure that the _VocabularyHarvester_ class handles the new field in its constructor.
4.  Launch the SOS.Hangfire.UI API and run _Vocabularies/Single/Create/[Vocabulary Name]_ to generate _[Vocabulary Name]Vocabulary.json_

**Import of [Vocabulary Name]Vocabulary.json to MongoDb**
1.  Copy _[Vocabulary Name]Vocabulary.json_ to the \Src\SOS.Import\Resources\Vocabularies directory. Make sure the Copy to Output Directory property has the _Copy if newer_ value.
2.  Launch the SOS.Hangfire.UI API and run _/Vocabularies/Import_. Once the job has been run, the vocabulary has entered the Vocabulary collection in the sos database.

**Process [Vocabulary]**
1.  Add a property of type _VocabularyValue_ to the _Observation_ class (or one that is referenced by the _Observation_ class) , for the new field.
2.  Update the process for the data providers that support this field. For Artportalen, the _SOS.Harvest.Processors.Artportalen.ArtportalenObservationProcessor_ class needs to be updated.

**Resolve vocabulary values**<br/>
Add the new field name to the following classes when resolving field mapped values:
1. _SOS.Observations.Api.Managers.ObservationManager_
2. _SOS.Export.IO.DwcArchive.DwcArchiveOccurrenceCsvWriter_ (if the field should be included in the DwC-A export)
3. _SOS.Lib.Helpers.VocabularyValueResolver_