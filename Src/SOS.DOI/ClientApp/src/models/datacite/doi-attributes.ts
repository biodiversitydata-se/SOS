import { ICreator } from './doi-creator';
import { IDescription } from './doi-description';
import { IIdentifier } from './doi-identifier';
import { ITitle } from './doi-title';

/**
 *
 */
export interface IAttributes {
  created: Date;
  creators: Array<ICreator>;
  descriptions: Array<IDescription>;
  doi: string;
  formats: Array<string>;
  identifiers: Array<IIdentifier>;
  language: string;
  publisher: string;
  publicationYear: number;
  state: string;
  titles: Array<ITitle>;
  url: string;
}
