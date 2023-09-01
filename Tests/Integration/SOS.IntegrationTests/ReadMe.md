# SOS integration tests
This is an overview of how the integration tests works. 
You can choose if you want to run the tests with databases temporarily created in Containers (default) or if you want to use on premise databases.

## 1. Databases running temporarily as containers in Docker (default)

1. When the tests are starting, one Elasticsearch database and one MongoDB database are created as containers and runs in Docker. The MongoDB is restored with data from the `~/Resources/MongoDb/mongodb-sos-dev.gz` file.
2. For each test, observations are created in raw data format (verbatim) with the properties you want them to have. 
3. Then the observations are processed using data from taxonomy, dictionaries and areas. This data are fetched from the MongoDB instance running in Docker. 
4. The observation is then added to the observation index in Elasticsearch. 
5. The endpoint you want to test is requested and a result is returned.
6. The result is verified.

## 2. Databases running on premise

1. First, observations are created in raw data format (verbatim) with the properties you want them to have. 
2. Then the observations are processed using data from taxonomy, dictionaries and areas. This data are fetched from MongoDB, but it could be moved to JSON files. 
3. The observation is then added to a special index in Elasticsearch which is only for integration tests. 
4. After that, the endpoint is called and a result is returned.
5. The result is verified.

### 2.1 Sequence diagram for running test with databases on premise
[![](https://mermaid.ink/img/pako:eNqNVE1v2zAM_SuCTgkQY9iORhEgadphh30A7XqZd2AsNjUgSx5FBw2K_vdRsYvGkZMmp4h675F8pPWiS29Q5zrgvxZdiasKNgR14ZT8oGXv2nqN1J0bIK7KqgHH6ptjFCRX3t1jYAXhMKRYYilp0UTsChiWbWUNUqQ9IK2FVB9djbF_rgPSdp_hFkr2tIsCC-LGE4NFlwJSmQNMuPaOyVvbVTJ-kyrcWAhyCAhUPkXiIHC1pk_zPwLvjl-yz1l04-8-fhV8yKp3o-JNyPx74nlMVrgu6Q_PqPxWqjuyO5emCdwGO9zRbTafJ07n6poQGGP4sM_JtJNICJmoJFm3_awOFU6XkE4jV7_IlxjCoIQx1emxA-NyvwMqwuBbElU1uYdn72bqwZewbi3QbiZGIUz33gehoFGVU9-92_jVch99az9VH3Wg6epHc4kFg73I1QotMi6sHQ6gm_hFCgszyDsZrSax7kjkEKuAUIEx4gt7xU8o9hh8vnBXP97Skk91Nv6x5eorDvZzubvblz3pqr-tLCP1HY5LpK6dkuxlBujsTGkdLe5ba_lsCWObM8I-55xMlVjPdI1UQ2XkjX6JpELLkGosdC5_DT5CFNOFexVo2xj5wm9MJdur80ewAWc6PuJ3O1fqnKnFN1D_zveo1_8WqzPI)](https://mermaid.live/edit#pako:eNqNVE1v2zAM_SuCTgkQY9iORhEgadphh30A7XqZd2AsNjUgSx5FBw2K_vdRsYvGkZMmp4h675F8pPWiS29Q5zrgvxZdiasKNgR14ZT8oGXv2nqN1J0bIK7KqgHH6ptjFCRX3t1jYAXhMKRYYilp0UTsChiWbWUNUqQ9IK2FVB9djbF_rgPSdp_hFkr2tIsCC-LGE4NFlwJSmQNMuPaOyVvbVTJ-kyrcWAhyCAhUPkXiIHC1pk_zPwLvjl-yz1l04-8-fhV8yKp3o-JNyPx74nlMVrgu6Q_PqPxWqjuyO5emCdwGO9zRbTafJ07n6poQGGP4sM_JtJNICJmoJFm3_awOFU6XkE4jV7_IlxjCoIQx1emxA-NyvwMqwuBbElU1uYdn72bqwZewbi3QbiZGIUz33gehoFGVU9-92_jVch99az9VH3Wg6epHc4kFg73I1QotMi6sHQ6gm_hFCgszyDsZrSax7kjkEKuAUIEx4gt7xU8o9hh8vnBXP97Skk91Nv6x5eorDvZzubvblz3pqr-tLCP1HY5LpK6dkuxlBujsTGkdLe5ba_lsCWObM8I-55xMlVjPdI1UQ2XkjX6JpELLkGosdC5_DT5CFNOFexVo2xj5wm9MJdur80ewAWc6PuJ3O1fqnKnFN1D_zveo1_8WqzPI)

# Creating test data

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