using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.UnitTests.TestHelpers.Factories;

namespace SOS.Lib.UnitTests.TestHelpers.Fixtures
{
    /// <summary>
    /// Reads a file created by the MessagePack library, that contains only the necessary fields
    /// for creating a taxon tree. Fields: DyntaxaTaxonId, ParentId, SecondaryParentIds, ScientificName
    /// </summary>
    public class MinimalDarwinCoreTaxaFixture : IDisposable
    {
        public MinimalDarwinCoreTaxaFixture()
        {
            Taxa = DarwinCoreTaxonFactory.CreateFromMessagePackFile<DarwinCoreTaxon>(@"Resources\AllTaxaInMongoDbWithMinimalFields.msgpck");
        }

        public void Dispose()
        {
            Taxa = null;
        }

        public IEnumerable<DarwinCoreTaxon> Taxa { get; private set; }
    }
}
