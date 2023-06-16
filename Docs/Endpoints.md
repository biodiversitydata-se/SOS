# Endpoints
The following endpoints exists in Species Observation System API (SOS):

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
| /Exports/Datasets 	| Exports_GetDatasetsList 	| GET 	| Get a list of data provider datasets (DwC-A) available for download. 	|
| /Exports/Download/Csv 	| Exports_DownloadCsv 	| POST 	| Download Csv export file. The limit is 25 000 observations. If you need to download more observations, use the OrderCsv endpoint. 	|
| /Exports/Download/GeoJson 	| Exports_DownloadGeoJson 	| POST 	| Download GeoJson export file. The limit is 25 000 observations. If you need to download more observations, use the OrderGeoJson endpoint. 	|
| /Exports/Download/Excel 	| Exports_DownloadExcel 	| POST 	| Download Excel export file. The limit is 25 000 observations. If you need to download more observations, use the OrderExcel endpoint. 	|
| /Exports/Download/DwC 	| Exports_DownloadDwC 	| POST 	| Download Darwin Core Archive (DwC-A) export file. The limit is 25 000 observations. If you need to download more observations, use the OrderDwC endpoint. 	|
| /Exports/Order/Csv 	| Exports_OrderCsv 	| POST 	| Starts Csv aynchronous file order. When the file is ready, you will receive an email containing a download link. The limit is 2 million observations. |
| /Exports/Order/GeoJson 	| Exports_OrderGeoJson 	| POST 	| Starts GeoJSON aynchronous file order. When the file is ready, you will receive an email containing a download link. The limit is 2 million observations. |
| /Exports/Order/Excel 	| Exports_OrderExcel 	| POST 	| Starts Excel aynchronous file order. When the file is ready, you will receive an email containing a download link. The limit is 2 million observations. |
| /Exports/Order/DwC 	| Exports_OrderDwC 	| POST 	| Starts Darwin Core Archive (DwC-A) aynchronous file order. When the file is ready, you will receive an email containing a download link. The limit is 2 million observations. |
| &nbsp;  	|  	|  	|  	|  	|
| **Jobs** 	| 	|  	|  	|  	|  
| /Jobs/\{jobId\}/Status 	| Jobs_GetStatus 	| GET 	| Get status of a job. Using this endpoint you can check the status of a file order. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Areas** 	| 	|  	|  	|  	|  
| /Areas | Areas_GetAreas | GET | Search for areas (regions). |
| /Areas/\{areaType\}/\{featureId\}/Export | Areas_GetExport | GET | Get an area as a zipped JSON file including its polygon. |
| &nbsp;  	|  	|  	|  	|  	|
| **Data providers & dataset** 	| 	|  	|  	|  	|  
| /DataProviders 	| DataProviders_GetDataProviders 	| GET 	| Get all data providers. 	|
| /DataProviders/\{providerId\}/LastModified 	| DataProviders_GetLastModifiedDateById 	| GET 	| Get latest modified date for a data provider. 	|
| /DataProviders/\{providerId\}/EML 	| DataProviders_GetEML 	| GET 	| Get provider EML file. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Vocabularies** 	| 	|  	|  	|  	|  
| /Vocabularies 	| Vocabularies_GetVocabularies 	| GET 	| Get all vocabularies. 	|
| /Vocabularies/\{vocabularyId\} 	| Vocabularies_GetVocabularyById 	| GET 	| Get a specific vocabulary. 	|
| /Vocabularies/ZipFile 	| Vocabularies_GetVocabulariesAsZipFile 	| GET 	| Get all vocabularies as zip file. 	|
| /Vocabularies/Projects 	| Vocabularies_GetProjects 	| GET 	| Get all Artportalen projects. 	|
| /Vocabularies/ObservationProperties 	| Vocabularies_GetObservationProperties 	| GET 	| Get all observation properties. 	|
| /Vocabularies/ObservationProperties/\{fieldSet\} 	| Vocabularies_GetObservationPropertiesByFieldSet 	| GET 	| Get observation properties that is part of a specific field set. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Taxon lists** 	| 	|  	|  	|  	|  
| /TaxonLists 	| TaxonLists_GetTaxonLists 	| GET 	| Get all Taxon list definitions. 	|
| /TaxonLists/\{taxonListId\}/Taxa 	| TaxonLists_GetTaxa 	| GET 	| Get all taxa in a taxon list. 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Locations** 	| 	|  	|  	|  	|  
| /Locations 	| Locations_GetLocationsByIds 	| GET 	| Get location info by id. 	|
