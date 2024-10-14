import { Provider } from "./providerinfo";

export class ProcessInfo {
  id: string;
  processFailCount: string;
  publicCount: string
  protectedCount: string;
  start: string;
  end: string;
  status: string;
  providersInfo: Provider[];
}
