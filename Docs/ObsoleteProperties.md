# Obsolete properties
The following properties are obsolete and will be removed in next SOS release (january/february 2023).


## Observation
| Obsolete Property                                            | Replaced by                                                    |
| :----------------------------------------------------------- | :------------------------------------------------------------- |
| Identification.Validated                                     | Identification.Verified                                        |
| Identification.ValidationStatus                              | Identification.VerificationStatus                              |
| Protected                                                    | Sensitive                                                      |
| Occurrence.IndividualId                                      | [deleted]                                                      |
| Occurrence.ProtectionLevel                                   | Occurrence.SensitivityCategory                                 |
| Taxon.Attributes.ProtectionLevel                             | Taxon.Attributes.SensitivityCategory                           |
| ArtportalenInternal.HasTriggeredValidationRules              | ArtportalenInternal.HasTriggeredVerificationRules              |
| ArtportalenInternal.HasAnyTriggeredValidationRuleWithWarning | ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning |
| ArtportalenInternal.LocationExternalId                       | Location.Attributes.ExternalId                                 |
| ArtportalenInternal.SpeciesGroupId                           | Taxon.Attributes.SpeciesGroup                                  |
| ArtportalenInternal.RegionalSightingStateId                  | [deleted]                                                      |