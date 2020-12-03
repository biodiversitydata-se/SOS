import { Provider } from "./provider";

export class ProcessInfo {
  id: string;
  count: string;
  start: string;
  end: string;
  status: string;
  providersInfo: Provider[];
}
