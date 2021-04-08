import { Provider } from "./provider";

export class ProcessInfo {
  id: string;
  publicCount: string
  protectedCount: string;
  start: string;
  end: string;
  status: string;
  providersInfo: Provider[];
}
