import { Term } from "./term";

export interface TermAggregation {
  name: string;
  terms: Term[];
}
