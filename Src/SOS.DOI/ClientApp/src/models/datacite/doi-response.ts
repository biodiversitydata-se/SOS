import { IResponseMeta } from './doi-responsemeta';

export interface IResponse<T> {
  data: T;
  meta: IResponseMeta;
}
