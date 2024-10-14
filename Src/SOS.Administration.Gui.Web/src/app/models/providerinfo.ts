export class Provider {
  dataProviderId: number;
  dataProviderIdentifier: string
  publicProcessCount: number;
  processFailCount: number;
  protectedProcessCount: number;
  processStart: Date;
  processEnd: Date;
  processStatus: string;
  harvestCount: number;
  harvestStart: Date;
  harvestEnd: Date;
  harvestNotes: string;
  harvestStatus: string;
  latestIncrementalPublicCount: number;
  latestIncrementalProtectedCount: number;
  latestIncrementalStart: Date;
  latestIncrementalEnd: Date;
  latestIncrementalStatus: string;
}
