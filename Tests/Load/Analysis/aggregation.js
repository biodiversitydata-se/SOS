import http from 'k6/http';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const taxonIds = JSON.parse(open('./data-taxonIds.json')).taxonIds;

export const options = JSON.parse(open('./options.json'));

export default function () {
  let url = 'https://sos-analysis-st.artdata.slu.se/internal/aggregation?aggregationField=event.startYear&take=10';
  const precisionThreshold = __ENV.PT;
  if (precisionThreshold){
    url += '&precisionThreshold=' + precisionThreshold
  }

  let filter = {
    "date": {
      "startDate": "2018-01-01T00:00:00",
      "endDate": "2023-12-01T23:59:59",
      "dateFilterType": "OnlyStartDate"
    },
    "taxon": {
      "includeUnderlyingTaxa": true,
      "ids": [
        randomItem(taxonIds)
      ]
    },
    "determinationFilter": "NotUnsureDetermination",
    "notRecoveredFilter": "NoFilter",
    "occurrenceStatus": "Present"
  };

  // Using a JSON string as body
  http.post(
    url, 
    JSON.stringify(filter), 
    {
      headers: { 
        'Content-Type': 'application/json'
      }
    }
  );
}
