export interface PagedObservations {
  records: Observation[];
  skip: number;
  take: number;
  totalCount: number;
}

export interface Observation {
  occurrenceId: string;
  dataSetId: string;
  dataSetName: string;
  lat: number;
  lon: number;
  diffusionRadius: number;
}
