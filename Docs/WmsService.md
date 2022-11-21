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

## Recommendations
- Since the maximum number of observations rendered is 5000, you should rather use the WFS service which can handle 100 000.

## Known problems
The WMS is using [GeoServer](https://geoserver.org/) and a plugin to GeoServer that has a [bug](https://github.com/ngageoint/elasticgeo/issues/122) leading to that requests sometimes stop being processed and no observations are returned until the server is restarted. This problem occurs about once a month. Currently we are restarting GeoServer once a day to try avoid that this problem affects users of the WMS.

## Support
In case of questions or problems, contact support at SLU Artdatabanken: artdatabanken@slu.se