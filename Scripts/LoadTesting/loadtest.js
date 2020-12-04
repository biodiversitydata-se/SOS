import { group, sleep, check } from "k6";
import http from "k6/http";

export let options = {
  stages: [
    { duration: '2m', target: 200 }, // simulate ramp-up of traffic from 1 to 100 users over 1 minute.
    { duration: '10m', target: 200 }, // stay at 100 users for 10 minutes
    { duration: '2m', target: 0 }, // ramp-down to 0 users
  ],
  thresholds: {
    RTT: ['p(99)<3000', 'p(70)<2500', 'avg<2000', 'med<1500', 'min<1000'],
    'Content OK': ['rate>0.95'],    
    Errors: ['count<100'],
  },
};

export default function main() {
  let headers = {
    pragma: "no-cache",
    "cache-control": "no-cache",
    accept: "text/plain",
    "x-api-version": "1.0",
    "user-agent":
      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
    "content-type": "application/json",    
    "sec-fetch-site": "same-origin",
    "sec-fetch-mode": "cors",
    "sec-fetch-dest": "empty",    
    "accept-encoding": "gzip, deflate, br",
    "accept-language": "sv-SE,sv;q=0.9,en-US;q=0.8,en;q=0.7,de;q=0.6",
    cookie:
      "_ga=GA1.2.1334924374.1540545808; _hjTLDTest=1; _hjid=08f18c7a-240e-4b83-a0ca-98c56f04ad71",
    "Content-Type": "application/json",
  };
  //let environmentUrl = "https://localhost:44380";
  let environmentUrl = "https://sos-search-st.artdata.slu.se";
  group("Searches", function () {
    group("Otters in Tranaes", function () {
      let response;
      let body = {
        date: {
          startDate: "1990-01-31T07:59:46.862Z",
          endDate: "2020-01-31T07:59:46.862Z",
          dateFilterType: "OnlyStartDate",
        },
        taxon: { taxonIds: [100077] },
        areaIds: [7, 283],
        onlyValidated: false,
        occurrenceStatus: "present",
        translationCultureCode: "sv-SE",
      };

      response = http.post(
        environmentUrl + "/Observations/Search?take=100&validateSearchFilter=true",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });

      // Automatically added sleep
      sleep(1);
    });
    group("Gaedda in Enkoeping", function () {
      let response;
      let body = {
        date: {
          startDate: "2008-07-16T00:00:00",
          endDate: "2008-08-18T00:00:00",          
        },
        taxon: { taxonIds: [206139] },
        areaIds: [164],
        onlyValidated: false,
        occurrenceStatus: "present",
        translationCultureCode: "sv-SE",
      };

      response = http.post(
        environmentUrl + "/Observations/Search?skip=0&take=2&validateSearchFilter=true",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });

      // Automatically added sleep
      sleep(1);
    });
    group("Hedgehogs in polygon region", function () {
      let response;
      let body = {
        "outputFields": [
            "dataProviderId",
            "occurrence.recordedBy",
            "occurrence.url",
            "occurrence.organismQuantity",
            "occurrence.isNotRediscoveredObservation",
            "occurrence.organismQuantityUnit",
            "occurrence.activity",
            "event.startDate",
            "event.endDate",
            "artportalenInternal.projects",
            "identification.validated",
            "location.locality",
            "location.decimalLongitude",
            "location.decimalLatitude",
            "location.coordinateUncertaintyInMeters",
            "taxon.dyntaxaTaxonId"
        ],
        "date": {
            "startDate": "2015-01-01T00:00:00+01:00",
            "endDate": "2020-11-24T00:00:00+01:00",
            "searchOnlyBetweenDates": true
        },
        "taxon": {
            "taxonIds": [
                100053
            ],
            "includeUnderlyingTaxa": true,
            "redListCategories": null
        },
        "geometry": {
            "maxDistanceFromPoint": 0.0,
            "geometries": [
                {
                    "type": "Polygon",
                    "coordinates": [
                        [
                            [
                                15.781606,
                                60.608542
                            ],
                            [
                                15.781606,
                                58.676938
                            ],
                            [
                                20.133112,
                                58.722599
                            ],
                            [
                                20.352885,
                                60.543775
                            ],
                            [
                                15.781606,
                                60.608542
                            ]
                        ]
                    ]
                }
            ],
            "usePointAccuracy": false
        },
        "onlyValidated": false,
        "translationCultureCode": "sv-SE"
    }

      response = http.post(
        environmentUrl + "/Observations/Search?take=100&validateSearchFilter=true",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    });
    group("Search birds(lom)  in a specific area", function () {
      let response;
      let body = {
        date: {
          startDate: "2015-01-01T00:00:00+01:00",
          endDate: "2020-01-31T07:59:46.862Z",
          dateFilterType: "OnlyStartDate",
        },
        taxon: {
          taxonIds: [100063, 100062, 6006170, 233863, 266774],
          includeUnderlyingTaxa: true,
        },
        areaIds: [208],
        onlyValidated: false,
        occurrenceStatus: "present",
        translationCultureCode: "sv-SE",
      };

      response = http.post(
        environmentUrl + "/Observations/Search?skip=0&take=1000&validateSearchFilter=true&sortBy=event.startDate&sortOrder=Desc",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    });

  });
  group("TaxonAggregation", function(){
    group("TaxonAggregation all taxon with bbox", function(){
      let response;
      let body = {
        date: {
          startDate: "1990-01-31T07:59:46.862Z",
          endDate: "2020-01-31T07:59:46.862Z",
        },
        onlyValidated: false,
        occurrenceStatus: "present",
        translationCultureCode: "sv-SE",
      };

      response = http.post(
        environmentUrl + "/Observations/TaxonAggregation?skip=0&take=500&bboxLeft=17.9296875&bboxTop=59.355596110016315&bboxRight=18.28125&bboxBottom=59.17592824927137",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })
  })
  group("GeoGridAggregation", function(){
    group("GeoGridAggregation all mammals with bbox", function(){
      let response;
      let body = {    
        date: {
            startDate:"1990-01-31T07:59:46.862Z",
          endDate:"2020-01-31T07:59:46.862Z"
        },
        taxon : {
            taxonIds:[4000107],
            includeUnderlyingTaxa : true
        },    
        onlyValidated:false,
        occurrenceStatus:"present",
        translationCultureCode:"sv-SE"    
    };

      response = http.post(
        environmentUrl + "/Observations/geogridaggregation?zoom=15&bboxLeft=17.9296875&bboxTop=59.355596110016315&bboxRight=18.28125&bboxBottom=59.17592824927137",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })
    group("GeoGridAggregation from application insights", function(){
      let response;
      let body = {
        "Areas":null,
        "AreaIds":null,
        "CountyIds":null,
        "DataProviderIds":null,
        "EndDate":"2020-01-31T07:59:46",
        "FieldTranslationCultureCode":"sv-SE",
        "GeometryFilter":null,
        "GenderIds":null,
        "IncludeUnderlyingTaxa":true,
        "IsFilterActive":true,
        "MunicipalityIds":null,
        "OnlyValidated":false,
        "PositiveSightings":true,
        "ProvinceIds":null,"RedListCategories":null,
        "SearchOnlyBetweenDates":false,
        "TypeFilter":0,
        "StartDate":"1990-01-31T07:59:46",
        "TaxonIds":[6034824,6034827,6034822,6034825,2002138,1001619,205986,1001618,102101,205985,205983,205982,205981,2002139,1001617,205980,2002137,1001616,100053,3000534,2003068,1009404,233623,233622,3000305,2002171,2002172,6011558,6011559,6011560,6007819,6011554,6011555,6007820,6007821,1001658,206044,1001657,6011557,206043,102103,6007818,1015254,257771,1001661,233627,100120,6006720,6006719,1001660,206046,1001659,206045,2002173,6011542,6011613,6011543,1009407,233628,1009406,6012025,233626,264917,246283,1009405,233625,1001656,206042,2002169,2002170,1001655,6010576,6037370,206041,6037368,6037369,6011593,233624,3000304,2002166,2002168,6037427,233173,1008687,233174,2002167,1008686,233172,1008685,233171,233170,233169,233168,233175,2002160,6037425,1008680,233159,2002162,1006631,100106,232475,2002165,1008684,233167,1008683,233166,1008682,233165,233164,233163,233162,233161,2002164,1008681,233160,2002163,1008679,233158,1008678,233156,2002161,1008675,233153,1008674,233152,1008673,233151,1008672,233150,1008671,233149,233148,1008670,233147,1008669,233146,1008668,233145,1008667,233144,3000303,2002153,6011582,6011583,6011584,6003380,6007826,6007827,6003381,6003382,2002158,1008688,233176,2002159,1012705,246126,1012702,246082,1016571,100104,6003882,1001654,100068,263711,263710,1001653,102708,100105,2002156,6037430,206031,1001651,206036,1001650,100077,1001649,100066,1001648,6011585,206033,1001647,206032,6011586,206030,206029,2002154,1001646,206028,1001644,206026,100005,1001643,6011549,267320,100024,233621,2002155,1001642,100145,6008221,2002151,6007814,6037429,6007816,6007815,2002152,1009403,233620,1001652,100057,3000299,6010511,2002140,1001626,206002,232267,1001625,100015,1001624,206000,1001623,205998,100051,1001622,100092,232266,1001621,205995,205994,100111,1001620,102102,205992,205991,205988,262167,100087,100086,100085,232474,6034821,3000302,2002149,2002150,1009408,233629,3000301,2002148,1001628,206005,233619,1001627,6050991,264345,206003,264344,206004,264346,3000300,6034849,2002143,1001630,6007508,102607,2003804,6037419,6037420,2003805,1015253,257770,2002144,6034811,6037422,1001641,100130,2002881,6034809,6034810,1001615,6034815,1008257,232133,6034812,6034817,206019,1001637,206018,206017,6034813,1001640,206022,1001639,206021,6034814,6034816,206016,1001638,206020,1001636,206015,2002145,1001614,1001635,100080,1001634,206013,1001633,6008456,206012,206011,1001632,206009,100121,2002141,2002146,6011580,6011581,6011570,6011571,1001631,100084,2002142,6007833,6037423,6007835,6011591,6011592,6007834,6007811,6007812,6007813,6007810,1001629,6007831,6007829,102606,6034823,6011587,6011588,6011589,6011590,4000107]
      };

      response = http.post(
        environmentUrl + "/Observations/geogridaggregation?zoom=15&bboxLeft=17.9296875&bboxTop=59.355596110016315&bboxRight=18.28125&bboxBottom=59.17592824927137",
        JSON.stringify(body),
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,        
      });
      // Automatically added sleep
      sleep(1);
    })
  })
  group("Areas", function(){
    group("Get All areas", function(){
      let response;      

      response = http.get(
        environmentUrl + "/areas?areaTypes[0]=province&areaTypes[1]=spa&areaTypes[2]=sci&areaTypes[3]=county&areaTypes[4]=waterArea&areaTypes[5]=municipality&areaTypes[6]=protectednature&skip=0&take=20000",        
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })
    group("Get Some areas", function(){
      let response;     

      response = http.get(
        environmentUrl + "/areas?areaTypes[0]=Province&skip=0&take=100000",        
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })
    group("Get Some areas 2", function(){
      let response;      

      response = http.get(
        environmentUrl + "/areas?areaTypes[0]=Province&areaTypes[1]=County&skip=0&take=100000",        
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })

    group("Export Area", function(){
      let response;      

      response = http.get(
        environmentUrl + "/areas/8060/export",        
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })   
  })
  group("DataProviders", function(){
    group("Get DataProviders", function(){
      let response;      

      response = http.get(
        environmentUrl + "/DataProviders",        
        {
          headers: headers,
          timeout:180000
        }
      );
      check(response, {
        'is status 200': (r) => r.status === 200,
      });
      // Automatically added sleep
      sleep(1);
    })   
  })
}