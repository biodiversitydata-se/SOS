# Endpoints

## Observations
| Resource URL | Name | Method | Description |
|:---|:---|:---|:---|
|**Observations**| | | | 
| /Observations/Search | Observations_ObservationsBySearch | POST | Get observations matching the provided search filter. |
| /Observations/Count | Observations_Count | POST | Count the number of observations matching the provided search filter. |
| /Observations/\{occurrenceId\} | Observations_GetObservationById | GET | Get a single observation. |
| /Observations/GeoGridAggregation | Observations_GeogridAggregation | POST | Aggregate observations into grid cells. |
| /Observations/TaxonAggregation | Observations_TaxonAggregation | POST | Aggregate observations by taxon. |
| &nbsp;  	|  	|  	|  	|  	|
| **Exports** 	| 	|  	|  	|  	|  
| /Exports/Datasets 	| Exports_GetDatasetsList 	| GET 	| Get a list of datasets available for download. 	|
| /Exports/PostRequest 	| Exports_PostRequest 	| POST 	| Create an asynchronous file order. When the file is ready, you will receive an email containing a download link. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Jobs** 	| 	|  	|  	|  	|  
| /Jobs/\{jobId\}/Status 	| Jobs_GetStatus 	| GET 	| Get status of a job. Using this endpoint you can check the status of a file order created with `Exports_PostRequest` 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Areas** 	| 	|  	|  	|  	|  
| /Areas | Areas_GetAreas | GET | Search for areas (regions). |
| /Areas/\{areaType\}/\{featureId\}/Export | Areas_GetExport | GET | Get an area as a zipped JSON file including its polygon. |
| &nbsp;  	|  	|  	|  	|  	|
| **Data providers** 	| 	|  	|  	|  	|  
| /DataProviders 	| DataProviders_GetDataProviders 	| GET 	| Get all data providers. 	|
| /DataProviders/\{providerId\}/LastModified 	| DataProviders_GetLastModifiedDateById 	| GET 	| Get latest modified date for a data provider. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Vocabularies** 	| 	|  	|  	|  	|  
| /Vocabularies 	| Vocabularies_GetVocabularies 	| GET 	| Get all vocabularies. 	|
| /Vocabularies/\{vocabularyId\} 	| Vocabularies_GetVocabularyById 	| GET 	| Get a specific vocabulary. 	|
| /Vocabularies/ZipFile 	| Vocabularies_GetVocabulariesAsZipFile 	| GET 	| Get all vocabularies as zip file. 	|
| /Vocabularies/Projects 	| Vocabularies_GetProjects 	| GET 	| Get all Artportalen projects. 	|