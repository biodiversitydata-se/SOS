using System;
using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;
using SOS.TestHelpers.Helpers;

namespace SOS.Lib.UnitTests.TestHelpers.Fixtures
{
    /// <summary>
    ///     Reads a file created by the MessagePack library, that contains only the necessary fields
    ///     for creating a taxon tree. Fields: DyntaxaTaxonId, ParentId, SecondaryParentIds, ScientificName
    /// </summary>
    public class ProcessedBasicTaxaFixture : IDisposable
    {
        public ProcessedBasicTaxaFixture()
        {
            Taxa = MessagePackHelper.CreateListFromMessagePackFile<ProcessedBasicTaxon>(
                @"Resources\AllProcessedBasicTaxa.msgpck");
        }

        public IEnumerable<ProcessedBasicTaxon> Taxa { get; private set; }

        public void Dispose()
        {
            Taxa = null;
        }
    }
}