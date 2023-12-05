import http from 'k6/http';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const url = 'https://sos-analysis-st.artdata.slu.se/internal/aggregation_simple?aggregationField=event.startYear&take=10&sortOrder=KeyDescending';
const taxonIds = [
    206011, 2002112, 103024, 103025, 103023, 103026, 103032, 100001, 100943, 101260,
    101248, 222135, 221100, 219680, 222110
];

export const options = {
  // Key configurations for avg load test in this section
  stages: [
    { duration: '1m', target: 100 }, // traffic ramp-up from 1 to 100 users over 5 minutes.
    { duration: '5m', target: 100 }, // stay at 100 users for 30 minutes
    { duration: '1m', target: 0 }, // ramp-down to 0 users
  ],
};

export default function () {
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