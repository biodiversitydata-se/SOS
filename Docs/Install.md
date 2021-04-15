## How to install
In order to install SOS you must fulfill these prerequisites

Prerequisite       						| Description						| Mandatory
| ------------- |-------------|-------------|
| MongoDB      | MongoDb to host Hangfire, sos-harvest and sos db | true |
| ElasticSearch      | Database to store all processed observations | true |
| Hangfire server      | Server to host SOS.Hangfire.Jobserver | true |
| Webserver      | IIS to host SOS.Administration.Api and SOS.Observations.Api | true |
| ZendTo      | When export function is used, ZendTo is used to inform user when file is created | false |
| IdentityServer      | To serach for protected observations, you must set up a identity server | false |

## SOS.Hangfire.JobServer
Job server is used to execute jobs. To populate your search index you have to use this service 

Mandatory:
Change properties in appsettings to fit your environment. HangfireDbConfiguration, ProcessDbConfiguration and VerbatimDbConfiguration should point to your MongoDB-server. 
SearchDbConfiguration should point to your ElasticSearch server

App secrets are stored in %APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json.

ImportConfiguration contains information about providers to harvest. To harvest from Artportalen you need access to their SQL-database. Some of the providers require authorization

Optional:
In order to create DwA-files and upload them to Blob Storage on process, you have to change BlobStorageConfiguration. DwcaFilesCreationConfiguration enables/disables file creation

To upload DOI's you need a account at DataCite (DataCiteServiceConfiguration).

ZendTo must be configured if Export function should be used

1. Build project using: dotnet.exe publish SOS.Hangfire.JobServer.csproj -c Release -o [target]
2. Copy target to your Hangfire server
3. Start SOS.Hangfire.Jobserver suggested using nssm, but as a console application will also work

## SOS.Administration.Api
Administration API is used to run diffrent jobs. e.g Harvest, processing and export

Mandatory:
Change properties in appsettings to fit your environment. HangfireDbConfiguration, ProcessDbConfiguration and VerbatimDbConfiguration should point to your MongoDB-server. 

App secrets are stored in IIS/Configuration Editor/system.webServer/aspNetCore/ApplicationHost.config/environmetVariables

1. Build project using: dotnet.exe publish SOS.Administration.Api.csproj -c Release -o [target]
2. Copy target to your webserver 

## SOS.Observations.Api
Main task in observation API is to search for observations. You could also get information about meta data like providers and vocabularies

Mandatory:
Change properties in appsettings to fit your environment. HangfireDbConfiguration and ProcessDbConfiguration should point to your MongoDB-server. 
SearchDbConfiguration should point to your ElasticSearch server

App secrets are stored in IIS/Configuration Editor/system.webServer/aspNetCore/ApplicationHost.config/environmetVariables

1. Build project using: dotnet.exe publish SOS.Observations.Api.csproj -c Release -o [target]
2. Copy target to your webserver 

## SOS.DOI
Search/view DOI's created by SOS system

Mandatory:
Change properties in appsettings to fit your environment. BlobStorageConfiguration and DataCiteServiceConfiguration must be configured. 

App secrets are stored in IIS/Configuration Editor/system.webServer/aspNetCore/ApplicationHost.config/environmetVariables

1. Build project using: dotnet.exe publish SOS.DOI.csproj -c Release -o [target]
2. Copy target to your webserver 
