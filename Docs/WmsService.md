# OGC Web Map Service (WMS)
All public observations that SOS harvests are available in a OGC Web Maps Service (WMS) described on this page. 

## See also
- [SOS WFS technical overview](WfsService.md)
- [SOS WFS get started guide](WfsServiceGetStarted.md)


## WMS service overview
| Name  	| Value 	|
|:---	|:---	|
| URL | https://sosgeo.artdata.slu.se/geoserver/SOS/wms |
| Max number of rendered features | 5 000 |

## Layers
| Name  	| Value 	|
|:---	|:---	|
| SpeciesObservations | All public observations |
| SpeciesObservationsInvasive | Species observations of invasive species in Sweden |
| SpeciesObservationsRedlisted | Species observations of redlisted species |

## Service details
- The service currently only supports public observations that SOS harvests. Observations of species classified sensitive (["nationellet skyddsklassade arter"](https://www.artdatabanken.se/var-verksamhet/fynddata/skyddsklassade-arter/)) can be accessed (given the user has permission) by direct requests to the SOS API (downloading results as shape files to import to a GIS application) or using the [Analysportalen](https://www.analysisportal.se/) (to be replaced by a new application during 2023).

## Recommendations
- Since the maximum number of observations rendered is 5000, you should rather use the WFS service which can handle 100 000.

## Known problems
The WMS is using [GeoServer](https://geoserver.org/) and a plugin to GeoServer that has a [bug](https://github.com/ngageoint/elasticgeo/issues/122) leading to that requests sometimes stop being processed and no observations are returned until the server is restarted. This problem occurs about once a month. Currently we are restarting GeoServer once a day to try avoid that this problem affects users of the WMS.

## Support
In case of questions or problems, contact support at SLU Artdatabanken: artdatabanken@slu.se
