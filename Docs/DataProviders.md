# Datasets from different data providers
The following datasets are harvested once a day by SOS. Use the endpoint `/DataProviders` *(DataProviders_GetDataProviders)* to get up to date information. More datasets (providers) are added continuously.

By default, searching observations will return observations from most datasets, except *Biologg* and *Swedish National Forest Inventory: Presence-absence vegetation data (1995-2017, discontinued sample)*. Those datasets are by default excluded to avoid returning either too many records of unknown quality, or avoid misinterpretation of data when included in general searches. [*Biologg*](https://artfakta.se/metadata/dataset-artobservationer/18) records have been collected without automated quality control, are largely unreviewed and lack quality flags. The [*discontinued sample*](https://artfakta.se/metadata/dataset-artobservationer/28) dataset from the National Forest Inventory partially overlaps in time with the current dataset [*1995-current*](https://artfakta.se/metadata/dataset-artobservationer/16) but includes a larger sample, potentially leading to misinterpretation of temporal trends in occurrences. You can include those datasets by explicitly requesting them using the data provider filter.

*Latest updated 2025-04-02*

| Id 	| Name 	| Organization 	| Number of observations 	|
|:---	|:---	|:--- |---:	|
| 1 | [Artportalen (Swedish Species Observation System)](https://artfakta.se/metadata/dataset-artobservationer/1) | SLU Swedish Species Information Centre | 111 614 306 |
| 3 | [Database for coastal fish (KUL)](https://artfakta.se/metadata/dataset-artobservationer/3) | Department of Aquatic Resources (SLU Aqua) | 326 529 |
| 4 | [Environmental data MVM](https://artfakta.se/metadata/dataset-artobservationer/4) | Environmental data MVM, SLU | 1 838 508 |
| 5 | [National register of survey test-fishing (NORS)](https://artfakta.se/metadata/dataset-artobservationer/5) | Department of Aquatic Resources (SLU Aqua) | 1 068 135 |
| 6 | [Swedish Electrofishing Register (SERS)](https://artfakta.se/metadata/dataset-artobservationer/6) | Department of Aquatic Resources (SLU Aqua) | 253 662 |
| 7 | [Sweden's Virtual Herbarium](https://artfakta.se/metadata/dataset-artobservationer/7) | Umeå University | 2 982 293 |
| 8 | [SHARK national archive for oceanographic data](https://artfakta.se/metadata/dataset-artobservationer/8) | Swedish Meteorological and Hydrological Institute (SMHI) | 2 084 443 |
| 9 | [Bird ringing centre in Sweden, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/9) | Swedish Museum of Natural History | 9 347 181 |
| 10 | [Entomological Collections (NHRS), via GBIF](https://artfakta.se/metadata/dataset-artobservationer/10) | Swedish Museum of Natural History | 79 786 |
| 11 | [Swedish Malaise Trap Project (SMTP), via GBIF](https://artfakta.se/metadata/dataset-artobservationer/11) | Station Linné | 147 859 |
| 12 | [Porpoise Observation Database, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/12) | Swedish Museum of Natural History | 4 924 |
| 13 | [Swedish Butterfly Monitoring Scheme (SeBMS), via GBIF](https://artfakta.se/metadata/dataset-artobservationer/13) | Department of Biology, Lund University | 169 653 |
| 15 | [Fishdata2 (FD2)](https://artfakta.se/metadata/dataset-artobservationer/15) | Department of Aquatic Resources (SLU Aqua) | 911 |
| 16 | [Swedish National Forest Inventory: Presence-absence vegetation data (1995-current)](https://artfakta.se/metadata/dataset-artobservationer/16) | Swedish National Forest Inventory, Department of Forest Resource Management, SLU | 8 527 596 |
| 17 | [Observation database of Redlisted species](https://artfakta.se/metadata/dataset-artobservationer/17) | SLU Swedish Species Information Centre | 697 349 |
| 18 | [Biologg](https://artfakta.se/metadata/dataset-artobservationer/18) | Overstellar Solutions AB | 95 026 |
| 19 | [iNaturalist Research-grade Observations Sweden](https://artfakta.se/metadata/dataset-artobservationer/19) | iNaturalist.org | 277 081 |
| 20 | [Swedish Bird Survey: Swedish coastal bird monitoring programme, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/20) | Department of Biology, Lund University | 31 725 |
| 21 | [Swedish Bird Survey: Summer point count routes, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/21) | Department of Biology, Lund University | 482 056 |
| 22 | [Swedish Bird Survey: Fixed routes, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/22) | Department of Biology, Lund University | 437 548 |
| 23 | [Breeding coastal birds in the Gulf of Bothnia, via GBIF](https://artfakta.se/metadata/dataset-artobservationer/23) | Department of Biology, Lund University | 59 942 |
| 24 | [National Meadow and Pasture Inventory (TUVA), via GBIF](https://artfakta.se/metadata/dataset-artobservationer/24) | Swedish Board of Agriculture | 254 190 |
| 25 | [National Inventories of Landscapes in Sweden (NILS)](https://artfakta.se/metadata/dataset-artobservationer/25) | Department of Forest Resource Management, SLU | 401 518 |
| 26 | [Collections of the Gothenburg Natural History Museum (GNM), via GBIF](https://artfakta.se/metadata/dataset-artobservationer/26) | Gothenburg Natural History Museum | 361 741 |
| 27 | [Lund University Biological Museum - Animal Collections](https://artfakta.se/metadata/dataset-artobservationer/27) | Lund University | 336 319 |
| 28 | [Swedish National Forest Inventory: Presence-absence vegetation data (1995-2017, discontinued sample)](https://artfakta.se/metadata/dataset-artobservationer/28) | Swedish National Forest Inventory, Department of Forest Resource Management, SLU | 6 316 684 |
|  |  |  | **148 196 965** |

