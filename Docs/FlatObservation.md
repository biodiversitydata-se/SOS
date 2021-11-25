# Observation (flat structure)
When creating [exports](Exports.md) to CSV, GeoJSON and Excel the hierarchical observation structure will be transformed to a flat observation structure.

In exports you can specify what field set you want to use: [Minimum](#minimum), [Extended](#extended), [AllWithValues](#allWithValues) or [All](#all).

You can also specify the kind of label (column header) you want to use: PropertyName, PropertyPath, Swedish or English.

### Minimum
| Property Name | Property Path | Swedish | English | Data type | Field set |
|:---	|:---	|:--- |---:	|
| OccurrenceId | Occurrence.OccurrenceId | Observation GUID | Occurrence Id | String | Minimum |
| DatasetName | DatasetName | Datakälla | Dataset | String | Minimum |
| StartDate | Event.StartDate | Startdatum | Start date | DateTime | Minimum |
| EndDate | Event.EndDate | Slutdatum | End date | DateTime | Minimum |
| Validated | Identification.Validated | Validerad | Verified | Boolean | Minimum |
| ValidationStatus | Identification.ValidationStatus.Value | Valideringsstatus | Verification status | String | Minimum |
| DecimalLatitude | Location.DecimalLatitude | Latitud | Decimal latitude | Double | Minimum |
| DecimalLongitude | Location.DecimalLongitude | Longitud | Decimal longitude | Double | Minimum |
| CoordinateUncertaintyInMeters | Location.CoordinateUncertaintyInMeters | Koordinatnoggrannhet (m) | Coordinate uncertainty (m) | Double | Minimum |
| Municipality | Location.Municipality.Name | Kommun | Municipality | String | Minimum |
| County | Location.County.Name | Län | County | String | Minimum |
| RecordedBy | Occurrence.RecordedBy | Observatör | Recorded by | String | Minimum |
| ReportedBy | Occurrence.ReportedBy | Rapportör | Reported by | String | Minimum |
| OccurrenceStatus | Occurrence.OccurrenceStatus.Value | Fyndstatus | Occurrence status | String | Minimum |
| IndividualCount | Occurrence.IndividualCount | Antal individer | Individual count | String | Minimum |
| OrganismQuantity | Occurrence.OrganismQuantity | Organismkvantitet | Organism quantity | String | Minimum |
| OrganismQuantityInt | Occurrence.OrganismQuantityInt | Organismkvantitet (heltal) | Organism quantity (integer) | Int32 | Minimum |
| OrganismQuantityUnit | Occurrence.OrganismQuantityUnit.Value | Organismkvantitetsenhet | Organism quantity unit | String | Minimum |
| DyntaxaTaxonId | Taxon.Id | Taxon Id | Taxon Id | Int32 | Minimum |
| ScientificName | Taxon.ScientificName | Vetenskapligt namn | Scientific name | String | Minimum |
| VernacularName | Taxon.VernacularName | Svenskt namn | Common name | String | Minimum |
| OrganismGroup | Taxon.Attributes.OrganismGroup | Organismgrupp | Organism group | String | Minimum |

### Extended
All fields in the Minimum field set, and also the following fields:

| Property Name | Property Path | Swedish | English | Data type | Field set |
|:---	|:---	|:--- |---:	|
| DataProviderId | DataProviderId | DataProviderId | DataProviderId | Int32 | Extended |
| Modified | Modified | Modifierad datum | Modified date | DateTime | Extended |
| BasisOfRecord | BasisOfRecord.Value | basisOfRecord | basisOfRecord | String | Extended |
| CollectionCode | CollectionCode | CollectionCode | CollectionCode | String | Extended |
| InstitutionCode | InstitutionCode.Value | Institution | Institution | String | Extended |
| OwnerInstitutionCode | OwnerInstitutionCode | OwnerInstitutionCode | OwnerInstitutionCode | String | Extended |
| RightsHolder | RightsHolder | Rättighetsinnehavare | Rights holder | String | Extended |
| PlainStartDate | Event.PlainStartDate | Startdatum [datumdel] | Start date [date part] | String | Extended |
| PlainEndDate | Event.PlainEndDate | Slutdatum [datumdel] | End date [date part] | String | Extended |
| PlainStartTime | Event.PlainStartTime | Starttid | Start time | String | Extended |
| PlainEndTime | Event.PlainEndTime | Sluttid | End time | String | Extended |
| EventRemarks | Event.EventRemarks | Besöksanmärkning | Event remarks | String | Extended |
| Habitat | Event.Habitat | Habitat | Habitat | String | Extended |
| DiscoveryMethod | Event.DiscoveryMethod.Value | Metod | Discovery method | String | Extended |
| SamplingEffort | Event.SamplingEffort | SamplingEffort | SamplingEffort | String | Extended |
| SamplingProtocol | Event.SamplingProtocol | SamplingProtocol | SamplingProtocol | String | Extended |
| SampleSizeUnit | Event.SampleSizeUnit | SampleSizeUnit | SampleSizeUnit | String | Extended |
| SampleSizeValue | Event.SampleSizeValue | SampleSizeValue | SampleSizeValue | String | Extended |
| EventMedia | Event.Media | Event media | Event media | String | Extended |
| EventMeasurementOrFacts | Event.MeasurementOrFacts | EventMeasurementOrFacts | EventMeasurementOrFacts | String | Extended |
| ConfirmedBy | Identification.ConfirmedBy | Konfirmerad av | Confirmed by | String | Extended |
| IdentifiedBy | Identification.IdentifiedBy | Determinatör | Identified by | String | Extended |
| UncertainIdentification | Identification.UncertainIdentification | Osäker artbestämning | Uncertain identification | Boolean | Extended |
| DeterminationMethod | Identification.DeterminationMethod.Value | Bestämningsmetod | Determination method | String | Extended |
| VerifiedBy | Identification.VerifiedBy | Validerad av | Verified by | String | Extended |
| GeodeticDatum | Location.GeodeticDatum | Geodetiskt datum | Geodetic datum | String | Extended |
| LocationId | Location.LocationId | Lokal Id | Location Id | String | Extended |
| Locality | Location.Locality | Lokal | Locality | String | Extended |
| Parish | Location.Parish.Name | Församling | Parish | String | Extended |
| Province | Location.Province.Name | Landskap | Province | String | Extended |
| IsNaturalOccurrence | Occurrence.IsNaturalOccurrence | Naturlig förekomst | IsNaturalOccurrence | Boolean | Extended |
| IsNeverFoundObservation | Occurrence.IsNeverFoundObservation | Aldrig funnen | IsNeverFoundObservation | Boolean | Extended |
| IsNotRediscoveredObservation | Occurrence.IsNotRediscoveredObservation | Ej återfunnen | IsNotRediscoveredObservation | Boolean | Extended |
| IsPositiveObservation | Occurrence.IsPositiveObservation | Påträffad | IsPositiveObservation | Boolean | Extended |
| OccurrenceRemarks | Occurrence.OccurrenceRemarks | Kommentarer | Occurrence remarks | String | Extended |
| ProtectionLevel | Occurrence.ProtectionLevel | Säkerhetsklass | Protection level | Int32 | Extended |
| Activity | Occurrence.Activity.Value | Aktivitet | Activity | String | Extended |
| Behavior | Occurrence.Behavior.Value | Beteende | Behaviour | String | Extended |
| Biotope | Occurrence.Biotope.Value | Biotop | Biotope | String | Extended |
| BiotopeDescription | Occurrence.BiotopeDescription | Biotopbeskrivning | Biotope description | String | Extended |
| AssociatedMedia | Occurrence.AssociatedMedia | Associerad media | Associated media | String | Extended |
| LifeStage | Occurrence.LifeStage.Value | Stadium | Life stage | String | Extended |
| ReproductiveCondition | Occurrence.ReproductiveCondition.Value | Reproduktionsstatus | Reproductive condition | String | Extended |
| Sex | Occurrence.Sex.Value | Kön | Sex | String | Extended |
| Url | Occurrence.Url | Url | Url | String | Extended |
| Length | Occurrence.Length | Längd (mm) | Length (mm) | Int32 | Extended |
| Weight | Occurrence.Weight | Vikt (g) | Weight (g) | Int32 | Extended |
| SubstrateName | Occurrence.Substrate.Name.Value | Substratnamn | Substrate name | String | Extended |
| Kingdom | Taxon.Kingdom | Rike | Kingdom | String | Extended |
| Phylum | Taxon.Phylum | Fylum | Phylum | String | Extended |
| Class | Taxon.Class | Klass | Class | String | Extended |
| Order | Taxon.Order | Ordning | Order | String | Extended |
| Family | Taxon.Family | Familj | Family | String | Extended |
| Genus | Taxon.Genus | Släkte | Genus | String | Extended |
| TaxonId | Taxon.TaxonId | Taxon GUID | Taxon Id (GUID) | String | Extended |
| TaxonRank | Taxon.TaxonRank | Taxonkategori | Taxon rank | String | Extended |
| ProtectedByLaw | Taxon.Attributes.ProtectedByLaw | Fridlyst | Protected by law | Boolean | Extended |
| TaxonProtectionLevel | Taxon.Attributes.ProtectionLevel.Value | Artens säkerhetsklass | Species sensitivity category | String | Extended |
| RedlistCategory | Taxon.Attributes.RedlistCategory | Rödlistekategori | Redlist category | String | Extended |
| MeasurementOrFacts | MeasurementOrFacts | MeasurementOrFacts | MeasurementOrFacts | String | Extended |
| Projects | Projects | Projekt | Projects | String | Extended |

### AllWithValues
All fields in the Minimum and Extended field sets, and also the following fields:

| Property Name | Property Path | Swedish | English | Data type | Field set |
|:---	|:---	|:--- |---:	|
| Protected | Protected | Skyddad | Protected | Boolean | AllWithValues |
| AccessRightsId | AccessRights.Id | Åtkomsträttigheter Id | Access rights Id | Int32 | AllWithValues |
| AccessRights | AccessRights.Value | Åtkomsträttigheter | Access rights | String | AllWithValues |
| BasisOfRecordId | BasisOfRecord.Id | basisOfRecord Id | basisOfRecord Id | Int32 | AllWithValues |
| InstitutionCodeId | InstitutionCode.Id | Institution Id | Institution Id | Int32 | AllWithValues |
| EventId | Event.EventId | Event Id | Event Id | String | AllWithValues |
| DiscoveryMethodId | Event.DiscoveryMethod.Id | Metod Id | Discovery method Id | Int32 | AllWithValues |
| VerbatimEventDate | Event.VerbatimEventDate | Ursprunglig tidaangivelse för besöket | Verbatim event date | String | AllWithValues |
| ValidationStatusId | Identification.ValidationStatus.Id | Valideringsstatus Id | Verification status Id | Int32 | AllWithValues |
| ConfirmedDate | Identification.ConfirmedDate | Konfirmationsdatum | Confirmed date | String | AllWithValues |
| DateIdentified | Identification.DateIdentified | Datum för determination | Date identfied | String | AllWithValues |
| DeterminationMethodId | Identification.DeterminationMethod.Id | Bestämningsmetod Id | Determination method Id | Int32 | AllWithValues |
| IdentificationQualifier | Identification.IdentificationQualifier | IdentificationQualifier | IdentificationQualifier | String | AllWithValues |
| TypeStatus | Identification.TypeStatus | TypeStatus | TypeStatus | String | AllWithValues |
| LocationRemarks | Location.LocationRemarks | Lokalanmärkning | Location remarks | String | AllWithValues |
| MunicipalityId | Location.Municipality.FeatureId | Kommun Id | Municipality Id | String | AllWithValues |
| CountyId | Location.County.FeatureId | Län Id | County Id | String | AllWithValues |
| ParishId | Location.Parish.FeatureId | Församling Id | Parish Id | String | AllWithValues |
| ProvinceId | Location.Province.FeatureId | Landskap Id | Province Id | String | AllWithValues |
| ContinentId | Location.Continent.Id | Kontinent Id | Continent Id | Int32 | AllWithValues |
| Continent | Location.Continent.Value | Kontinent | Continent | String | AllWithValues |
| CountryId | Location.Country.Id | Land Id | Country Id | Int32 | AllWithValues |
| Country | Location.Country.Value | Land | Country | String | AllWithValues |
| CountryCode | Location.CountryCode | Landkod | Country Code | String | AllWithValues |
| GeoreferencedBy | Location.GeoreferencedBy | Koordinatsatt av | Georeferenced by | String | AllWithValues |
| GeoreferencedDate | Location.GeoreferencedDate | Koordinatsatt datum | Georeferenced Date | String | AllWithValues |
| GeoreferenceRemarks | Location.GeoreferenceRemarks | GeoreferenceRemarks | GeoreferenceRemarks | String | AllWithValues |
| HigherGeography | Location.HigherGeography | higherGeography | Higher Geography | String | AllWithValues |
| Island | Location.Island | Ö | Island | String | AllWithValues |
| MaximumDepthInMeters | Location.MaximumDepthInMeters | Maxdjup (m) | Maximum depth (m) | Double | AllWithValues |
| MaximumElevationInMeters | Location.MaximumElevationInMeters | Maxhöjd (m) | Maximum elevation (m) | Double | AllWithValues |
| MinimumDepthInMeters | Location.MinimumDepthInMeters | Mindjup (m) | Minimum depth (m) | Double | AllWithValues |
| MinimumElevationInMeters | Location.MinimumElevationInMeters | Minhöjd (m) | Minimum elevation (m) | Double | AllWithValues |
| VerbatimCoordinateSystem | Location.VerbatimCoordinateSystem | Ursprungligt koordinatsystem | verbatimCoordinateSystem | String | AllWithValues |
| VerbatimLatitude | Location.VerbatimLatitude | Ursprunglig latitud | verbatimLatitude | String | AllWithValues |
| VerbatimLongitude | Location.VerbatimLongitude | Ursprunglig longitud | verbatimLongitude | String | AllWithValues |
| VerbatimSRS | Location.VerbatimSRS | verbatimSRS | verbatimSRS | String | AllWithValues |
| WaterBody | Location.WaterBody | Vattenområde | Water body | String | AllWithValues |
| OccurrenceStatusId | Occurrence.OccurrenceStatus.Id | Fyndstatus Id | Occurrence status Id | Int32 | AllWithValues |
| OrganismQuantityUnitId | Occurrence.OrganismQuantityUnit.Id | Organismkvantitetsenhet Id | Organism quantity unit Id | Int32 | AllWithValues |
| ActivityId | Occurrence.Activity.Id | Aktivitet Id | Activity Id | Int32 | AllWithValues |
| BehaviorId | Occurrence.Behavior.Id | Beteende Id | Behaviour Id | Int32 | AllWithValues |
| BiotopeId | Occurrence.Biotope.Id | Biotop Id | Biotope Id | Int32 | AllWithValues |
| LifeStageId | Occurrence.LifeStage.Id | Stadium Id | Life stage Id | Int32 | AllWithValues |
| ReproductiveConditionId | Occurrence.ReproductiveCondition.Id | Reproduktionsstatus Id | Reproductive condition Id | Int32 | AllWithValues |
| SexId | Occurrence.Sex.Id | Kön Id | Sex Id | Int32 | AllWithValues |
| SubstrateDescription | Occurrence.Substrate.Description | Substratbeskrivning | SubstrateDescription | String | AllWithValues |
| SubstrateId | Occurrence.Substrate.Id | Substrat | Substrate | Int32 | AllWithValues |
| SubstrateNameId | Occurrence.Substrate.Name.Id | Substratnamn Id | Substrate name Id | Int32 | AllWithValues |
| SubstrateQuantity | Occurrence.Substrate.Quantity | Substrat kvantitet | Substrate quantity | Int32 | AllWithValues |
| SubstrateSpeciesDescription | Occurrence.Substrate.SpeciesDescription | Substrat artbeskrivning | Substrate species description | String | AllWithValues |
| SubstrateSpeciesId | Occurrence.Substrate.SpeciesId | Substrat art id | Substrate species id | Int32 | AllWithValues |
| SubstrateSpeciesScientificName | Occurrence.Substrate.SpeciesScientificName | Substrat art vetenskapligt namn | Substrate species scientific name | String | AllWithValues |
| SubstrateSpeciesVernacularName | Occurrence.Substrate.SpeciesVernacularName | Substrat art svenskt namn | Substrate species vernacular name | String | AllWithValues |
| BirdNestActivityId | Occurrence.BirdNestActivityId | Häckningskriterie Id | BirdNestActivityId | Int32 | AllWithValues |
| CatalogNumber | Occurrence.CatalogNumber | Ursprungkällans observationsid | Catalog number | String | AllWithValues |
| CatalogId | Occurrence.CatalogId | CatalogId | CatalogId | Int32 | AllWithValues |
| AssociatedReferences | Occurrence.AssociatedReferences | Associerade referenser | Associated References | String | AllWithValues |
| IndividualId | Occurrence.IndividualId | Individnummer | Individual Id | String | AllWithValues |
| Media | Occurrence.Media | Media | Media | String | AllWithValues |
| Preparations | Occurrence.Preparations | Preparations | Preparations | String | AllWithValues |
| RecordNumber | Occurrence.RecordNumber | Rapportörens observationsId | Record number | String | AllWithValues |
| ReportedDate | Occurrence.ReportedDate | Rapporterad (Datum) | Reported date | DateTime | AllWithValues |
| AcceptedNameUsageId | Taxon.AcceptedNameUsageId | AcceptedNameUsageId | AcceptedNameUsageId | String | AllWithValues |
| BirdDirective | Taxon.BirdDirective | BirdDirective | BirdDirective | Boolean | AllWithValues |
| HigherClassification | Taxon.HigherClassification | Klassificering | Higher classification | String | AllWithValues |
| ScientificNameAuthorship | Taxon.ScientificNameAuthorship | Auktor | Scientific name authorship | String | AllWithValues |
| NomenclaturalStatus | Taxon.NomenclaturalStatus | Nomenklaturisk status | Nomenclatural status | String | AllWithValues |
| OriginalNameUsageId | Taxon.OriginalNameUsageId | OriginalNameUsageId | OriginalNameUsageId | String | AllWithValues |
| ParentNameUsageId | Taxon.ParentNameUsageId | ParentNameUsageId | ParentNameUsageId | String | AllWithValues |
| SecondaryParentDyntaxaTaxonIds | Taxon.SecondaryParentDyntaxaTaxonIds | SecondaryParentDyntaxaTaxonIds | SecondaryParentDyntaxaTaxonIds | String | AllWithValues |
| TaxonomicStatus | Taxon.TaxonomicStatus | Taxonomisk status | Taxonomic status | String | AllWithValues |
| TaxonRemarks | Taxon.TaxonRemarks | Taxonkonceptetets definition | Taxon remarks | String | AllWithValues |
| ActionPlan | Taxon.Attributes.ActionPlan | ÅGP | Action program | String | AllWithValues |
| DisturbanceRadius | Taxon.Attributes.DisturbanceRadius | Artens störningsradie | Species disturbance radius | Int32 | AllWithValues |
| DyntaxaTaxonIdAttributes | Taxon.Attributes.DyntaxaTaxonId | Dyntaxa taxon id | Dyntaxa Taxon Id | Int32 | AllWithValues |
| Natura2000HabitatsDirectiveArticle2 | Taxon.Attributes.Natura2000HabitatsDirectiveArticle2 | Habitatdirektivet bilaga 2 | Habitats directive Annex 2 | Boolean | AllWithValues |
| Natura2000HabitatsDirectiveArticle4 | Taxon.Attributes.Natura2000HabitatsDirectiveArticle4 | Habitatdirektivet bilaga 4 | Habitats directive Annex 4 | Boolean | AllWithValues |
| Natura2000HabitatsDirectiveArticle5 | Taxon.Attributes.Natura2000HabitatsDirectiveArticle5 | Habitatdirektivet bilaga 5 | Habitats directive Annex 5 | Boolean | AllWithValues |
| ParentDyntaxaTaxonId | Taxon.Attributes.ParentDyntaxaTaxonId | ParentDyntaxaTaxonId | ParentDyntaxaTaxonId | Int32 | AllWithValues |
| TaxonProtectionLevelId | Taxon.Attributes.ProtectionLevel.Id | Artens säkerhetsklass Id | Species sensitivity category Id | Int32 | AllWithValues |
| SortOrder | Taxon.Attributes.SortOrder | Taxon sorteringsordning | Taxon sort order | Int32 | AllWithValues |
| SwedishHistory | Taxon.Attributes.SwedishHistory | Invandringshistoria | Immigration history | String | AllWithValues |
| SwedishOccurrence | Taxon.Attributes.SwedishOccurrence | Svensk förekomst | Swedish occurence | String | AllWithValues |
| TaxonSynonyms | Taxon.Attributes.Synonyms | Synonymer | Synonyms | String | AllWithValues |
| VernacularNames | Taxon.Attributes.VernacularNames | Trivialnamn | Vernacular names | String | AllWithValues |
| DatasetId | DatasetId | Datakälla (GUID) | Dataset Id | String | AllWithValues |
| DynamicProperties | DynamicProperties | DynamicProperties | DynamicProperties | String | AllWithValues |
| InstitutionId | InstitutionId | InstitutionId | InstitutionId | String | AllWithValues |
| Language | Language | Språk | Language | String | AllWithValues |
| License | License | Licens | License | String | AllWithValues |
| PrivateCollection | PrivateCollection | Privat samling | Private collection | String | AllWithValues |
| PublicCollection | PublicCollection | Publik samling | Public collection | String | AllWithValues |
| SpeciesCollectionLabel | SpeciesCollectionLabel | SpeciesCollectionLabel | SpeciesCollectionLabel | String | AllWithValues |

### All
All fields in the Minimum, Extended and AllWithValues field sets, and also the following fields:

| Property Name | Property Path | Swedish | English | Data type | Field set |
|:---	|:---	|:--- |---:	|
| CollectionId | CollectionId | CollectionId | CollectionId | String | All |
| FieldNotes | Event.FieldNotes | Fältanteckningar | Field notes | String | All |
| FieldNumber | Event.FieldNumber | FieldNumber | FieldNumber | String | All |
| ParentEventId | Event.ParentEventId | ParentEventId | ParentEventId | String | All |
| IdentificationId | Identification.IdentificationId | Determinationens Id | Identification Id | String | All |
| IdentificationReferences | Identification.IdentificationReferences | IdentificationReferences | IdentificationReferences | String | All |
| IdentificationRemarks | Identification.IdentificationRemarks | Valideringkommentarer | Identification remarks | String | All |
| CoordinatePrecision | Location.CoordinatePrecision | Koordinatprecission | Coordinate precision | Double | All |
| FootprintSpatialFit | Location.FootprintSpatialFit | FootprintSpatialFit | FootprintSpatialFit | String | All |
| FootprintSRS | Location.FootprintSRS | FootprintSRS | FootprintSRS | String | All |
| FootprintWKT | Location.FootprintWKT | FootprintWKT | FootprintWKT | String | All |
| GeoreferenceProtocol | Location.GeoreferenceProtocol | Koordinatsättningsmetod | Georeference protocol | String | All |
| GeoreferenceSources | Location.GeoreferenceSources | GeoreferenceSources | GeoreferenceSources | String | All |
| GeoreferenceVerificationStatus | Location.GeoreferenceVerificationStatus | GeoreferenceVerificationStatus | GeoreferenceVerificationStatus | String | All |
| HigherGeographyId | Location.HigherGeographyId | higher Geography Id | Higher Geography Id | String | All |
| IslandGroup | Location.IslandGroup | Ögrupp | Island group | String | All |
| LocationAccordingTo | Location.LocationAccordingTo | LocationAccordingTo | LocationAccordingTo | String | All |
| MaximumDistanceAboveSurfaceInMeters | Location.MaximumDistanceAboveSurfaceInMeters | Max höjd över havet (m) | maximumDistanceAboveSurfaceInMeters | Double | All |
| MinimumDistanceAboveSurfaceInMeters | Location.MinimumDistanceAboveSurfaceInMeters | Min höjd över havet (m) | minimumDistanceAboveSurfaceInMeters | Double | All |
| PointRadiusSpatialFit | Location.PointRadiusSpatialFit | PointRadiusSpatialFit | PointRadiusSpatialFit | String | All |
| VerbatimCoordinates | Location.VerbatimCoordinates | Ursprungliga koordinater | Verbatim coordinates | String | All |
| VerbatimDepth | Location.VerbatimDepth | verbatimDepth | verbatimDepth | String | All |
| VerbatimElevation | Location.VerbatimElevation | verbatimElevation | verbatimElevation | String | All |
| VerbatimLocality | Location.VerbatimLocality | Ursprungligt lokalnamn | Verbatim locality | String | All |
| AssociatedOccurrences | Occurrence.AssociatedOccurrences | Relaterade observationer | Associated Occurrences | String | All |
| AssociatedSequences | Occurrence.AssociatedSequences | Genetiskt material | Associated Sequences | String | All |
| AssociatedTaxa | Occurrence.AssociatedTaxa | Andra taxa | Associated taxa | String | All |
| Disposition | Occurrence.Disposition | Disposition | Disposition | String | All |
| EstablishmentMeansId | Occurrence.EstablishmentMeans.Id | Etableringssätt Id | Establishment means Id | Int32 | All |
| EstablishmentMeans | Occurrence.EstablishmentMeans.Value | Etableringssätt | Establishment means | String | All |
| OtherCatalogNumbers | Occurrence.OtherCatalogNumbers | Övriga observationsId | Other catalog numbers | String | All |
| AcceptedNameUsage | Taxon.AcceptedNameUsage | AcceptedNameUsage | AcceptedNameUsage | String | All |
| InfraspecificEpithet | Taxon.InfraspecificEpithet | Undertaxonepitet | Infraspecific epitet | String | All |
| NameAccordingTo | Taxon.NameAccordingTo | Namn enligt | Name according to | String | All |
| NameAccordingToId | Taxon.NameAccordingToId | NameAccordingToId | NameAccordingToId | String | All |
| NamePublishedIn | Taxon.NamePublishedIn | Namnreferens | Name published in | String | All |
| NamePublishedInId | Taxon.NamePublishedInId | NamePublishedInId | NamePublishedInId | String | All |
| NamePublishedInYear | Taxon.NamePublishedInYear | Namnets publiceringsår | Name published in year | String | All |
| NomenclaturalCode | Taxon.NomenclaturalCode | Nomenklaturkod | Nomenclatural code | String | All |
| OriginalNameUsage | Taxon.OriginalNameUsage | OriginalNameUsage | OriginalNameUsage | String | All |
| ParentNameUsage | Taxon.ParentNameUsage | ParentNameUsage | ParentNameUsage | String | All |
| ScientificNameId | Taxon.ScientificNameId | Vetenskapligt namn (GUID) | Scientific Name Id | String | All |
| SpecificEpithet | Taxon.SpecificEpithet | Artepitet | Specific epitet | String | All |
| Subgenus | Taxon.Subgenus | Undersläkte | Subgenus | String | All |
| TaxonConceptId | Taxon.TaxonConceptId | Rekommenderad Taxon GUID | Taxon Concept Id (GUID) | String | All |
| VerbatimTaxonRank | Taxon.VerbatimTaxonRank | Ursprunglig taxonkategori | Verbatim taxon rank | String | All |
| BibliographicCitation | BibliographicCitation | BibliographicCitation | BibliographicCitation | String | All |
| DataGeneralizations | DataGeneralizations | DataGeneralizations | DataGeneralizations | String | All |
| InformationWithheld | InformationWithheld | InformationWithheld | InformationWithheld | String | All |
| References | References | Referenser | References | String | All |
| TypeId | Type.Id | Typ Id | Type Id | Int32 | All |
| Type | Type.Value | Typ | Type | String | All |
| Bed | GeologicalContext.Bed | GeologicalBed | GeologicalBed | String | All |
| EarliestAgeOrLowestStage | GeologicalContext.EarliestAgeOrLowestStage | EarliestAgeOrLowestStage | EarliestAgeOrLowestStage | String | All |
| EarliestEonOrLowestEonothem | GeologicalContext.EarliestEonOrLowestEonothem | EarliestEonOrLowestEonothem | EarliestEonOrLowestEonothem | String | All |
| EarliestEpochOrLowestSeries | GeologicalContext.EarliestEpochOrLowestSeries | EarliestEpochOrLowestSeries | EarliestEpochOrLowestSeries | String | All |
| EarliestEraOrLowestErathem | GeologicalContext.EarliestEraOrLowestErathem | EarliestEraOrLowestErathem | EarliestEraOrLowestErathem | String | All |
| EarliestGeochronologicalEra | GeologicalContext.EarliestGeochronologicalEra | EarliestGeochronologicalEra | EarliestGeochronologicalEra | String | All |
| EarliestPeriodOrLowestSystem | GeologicalContext.EarliestPeriodOrLowestSystem | EarliestPeriodOrLowestSystem | EarliestPeriodOrLowestSystem | String | All |
| Formation | GeologicalContext.Formation | Geologisk formation | Geological formation | String | All |
| GeologicalContextId | GeologicalContext.GeologicalContextId | GeologicalContextId | GeologicalContextId | String | All |
| Group | GeologicalContext.Group | Geologisk grupp | Geological group | String | All |
| HighestBiostratigraphicZone | GeologicalContext.HighestBiostratigraphicZone | HighestBiostratigraphicZone | HighestBiostratigraphicZone | String | All |
| LatestAgeOrHighestStage | GeologicalContext.LatestAgeOrHighestStage | LatestAgeOrHighestStage | LatestAgeOrHighestStage | String | All |
| LatestEonOrHighestEonothem | GeologicalContext.LatestEonOrHighestEonothem | LatestEonOrHighestEonothem | LatestEonOrHighestEonothem | String | All |
| LatestEpochOrHighestSeries | GeologicalContext.LatestEpochOrHighestSeries | LatestEpochOrHighestSeries | LatestEpochOrHighestSeries | String | All |
| LatestEraOrHighestErathem | GeologicalContext.LatestEraOrHighestErathem | LatestEraOrHighestErathem | LatestEraOrHighestErathem | String | All |
| LatestGeochronologicalEra | GeologicalContext.LatestGeochronologicalEra | LatestGeochronologicalEra | LatestGeochronologicalEra | String | All |
| LatestPeriodOrHighestSystem | GeologicalContext.LatestPeriodOrHighestSystem | LatestPeriodOrHighestSystem | LatestPeriodOrHighestSystem | String | All |
| LithostratigraphicTerms | GeologicalContext.LithostratigraphicTerms | LithostratigraphicTerms | LithostratigraphicTerms | String | All |
| LowestBiostratigraphicZone | GeologicalContext.LowestBiostratigraphicZone | LowestBiostratigraphicZone | LowestBiostratigraphicZone | String | All |
| Member | GeologicalContext.Member | Geologisk member | Geological member | String | All |
| MaterialSampleId | MaterialSample.MaterialSampleId | MaterialSampleId | MaterialSampleId | String | All |
| OrganismId | Organism.OrganismId | OrganismId | OrganismId | String | All |
| OrganismName | Organism.OrganismName | OrganismName | OrganismName | String | All |
| OrganismScope | Organism.OrganismScope | OrganismScope | OrganismScope | String | All |
| AssociatedOrganisms | Organism.AssociatedOrganisms | AssociatedOrganisms | AssociatedOrganisms | String | All |
| PreviousIdentifications | Organism.PreviousIdentifications | Tidigare indtentifieringar | Previous identifications | String | All |
| OrganismRemarks | Organism.OrganismRemarks | OrganismRemarks | OrganismRemarks | String | All |
