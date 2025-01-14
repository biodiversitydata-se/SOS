/*
  Run aggregation
  k6 run -e URL="https://sos-analysis-st.wt.artdata.slu.se/internal/aggregation?aggregationField=event.startYear&take=10&precisionThreshold=3000" aggregation.js
  k6 run -e URL="https://sos-analysis-st.wt.artdata.slu.se/internal/aggregation_simple?aggregationField=event.startYear&take=10&sortOrder=KeyDescending&precisionThreshold=40000" aggregation.js
*/ 
import http from 'k6/http';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const taxonIds = JSON.parse(open('./data-taxonIds.json')).taxonIds;

export const options = JSON.parse(open('./options.json'));

export default function () {
  const url = __ENV.URL;
  
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
