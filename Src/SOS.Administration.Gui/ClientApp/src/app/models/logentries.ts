import { LogEntry } from "./logentry";
import { TermAggregation } from "./termaggregation";

export interface LogEntries {
  logEntries: LogEntry[];
  aggregations: TermAggregation[];
}
