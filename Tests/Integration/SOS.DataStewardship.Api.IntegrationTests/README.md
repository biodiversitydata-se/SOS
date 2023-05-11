# Data stewardship API integration tests
The integration tests uses the Test Containers library.


## Creating a sos-dev MongoDb dump file
1. Create a dump file of the sos-dev database running in test environment: 
   `"mongodump.exe" --uri="mongodb://<user>:<password>@artmongo2-1-test.artdata.slu.se:27017/sos-dev --archive=mongodb-sos-dev.gz --gzip`
2. Now you have a lot of geometries in that that file. In order of get rid of them, do the following:
    1. Replace the `mongodb-sos-dev.gz` in the `Resources` directory.
    2. Start an integration test in ordet to spin up the sos-dev MongoDb docker container with the database restored.
    3. Connect to the sos-dev db running in the container using for example NoSQLBooster and delete the `Area` and `Area.files` collections.
    4. Uncomment the `UseSimplifiedEconomicZoneOfSweden(areaGeometries);` line in `AreaHarvester.HarvestAreasAsync()`.
    5. Change `appsettings.local.json` in `SOS.Hangfire.JobServer` to use the sos-dev MongoDb docker container.
    6. Start the Hangfire server locally and run `RunAreasHarvestJob`.
    7. Start a shell in the container and run `mongodump --username=mongo --password=mongo --gzip --db=sos-dev --archive=mongodb-sos-dev.gz --authenticationDatabase=admin`
    8. Copy the file to your computer `docker cp <container-id>:/mongodb-sos-dev.gz .` and add it to the Resource directory.