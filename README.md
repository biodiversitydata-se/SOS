## How to harvest
Navigate to the handle job page. You can run or schedule any of the following  **HarvestJobs**:

Job       						| Description           					| Parameters
| ------------- |-------------| -----|
| Geo/Run      | Harvest spatial data used for processing |  |
| Geo/Schedule/Daily      | Schedule daily harvest of spatial data | **hour**: Start hour, **minute**: Start minute |
| KUL/Run | Harvest data from KUL      | |
| KUL/Schedule/Daily      | Schedule daily harvest of KUL | **hour**: Start hour, **minute**: Start minute |
| SpeciesPortal/Run      | Harvest data from Artportalen |  |
| SpeciesPortal/Schedule/Daily | Schedule daily harvest of Artportalen      |**hour**: Start hour, **minute**: Start minute  |
| Taxon/Run      | Harvest taxon information from taxon service for processing     |  |
| Taxon/Schedule/Daily | Schedule daily harvest of taxon information      | **hour**: Start hour, **minute**: Start minute |

## How to process
Navigate to the handle job page. You can run or schedule any of the following  **ProcessJobs**:

Job       						| Description           					| Parameters
| ------------- |-------------| -----|
| Run      | Process data to Darwin Core|**sources**: Bitflag for sources: 1 - Artportalen, 2 - Träd och musselportalen, 4 - KUL. Example: 5 (1+4) for Artportalen and KUL, **toggleInstanceOnSuccess**: True to switch database after completion, false to not switch database after completion  |
| Daily      | Schedule process to Darwin Core | **sources**: Same as above, **toggleInstanceOnSuccess**: Same as above, **hour**: Start hour, **minute**: Start minute |

## How to export
Navigate to the handle job page. You can run or schedule any of the following  **ExportJobs**:

Job       						| Description           					| Parameters
| ------------- |-------------| -----|
| Run      | Export the processed file to Darwin Core. The file will be uploaded to Microsoft Azure.| |
| Daily      | Schedule export file. | **hour**: Start hour, **minute**: Start minute |

## Monitoration
Navigate to the monitor page in Hangfire.

Click the tab Jobs to see running, scheduled, completed, failed jobs etc.