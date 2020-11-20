import { Provider } from "./provider";

export class ProcessInfo {
  id: string;
  count: string;
  start: Date;
  end: Date;
  status: string;
  providersInfo: Provider[];
}
