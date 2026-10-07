using FluentAssertions;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using System.Collections.Generic;
using Xunit;

namespace SOS.Lib.UnitTests.Models.TaxonTree;

public class TaxonTreeTests
{
    [Theory]
    [InlineData(false, new[] { 2, 3, 4 })]
    [InlineData(true, new[] { 1, 2, 3, 4 })]
    public void GetUnderlyingTaxonIds_ReturnsDescendantsAndIncludesSelfOnlyWhenRequested(bool includeSelf, int[] expected)
    {
        var tree = CreateTree();

        var result = tree.GetUnderlyingTaxonIds(1, includeSelf);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(999)]
    public void GetUnderlyingTaxonIds_KeepsLeafOrUnknownIdSoTheSearchDoesNotLoseItsTaxonConstraint(int taxonId)
    {
        var tree = CreateTree();

        var result = tree.GetUnderlyingTaxonIds(taxonId, false);

        result.Should().Equal(taxonId);
    }

    private static TaxonTree<IBasicTaxon> CreateTree() =>
        TaxonTreeFactory.CreateTaxonTree(new Dictionary<int, IBasicTaxon>
        {
            [0] = new BasicTaxon { Id = 0, ScientificName = "Root" },
            [1] = new BasicTaxon { Id = 1, ScientificName = "Genus", Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 0 } },
            [2] = new BasicTaxon { Id = 2, ScientificName = "First species", Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 1 } },
            [3] = new BasicTaxon { Id = 3, ScientificName = "Second species", Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 1 } },
            [4] = new BasicTaxon { Id = 4, ScientificName = "Subspecies", Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 2 } },
            [5] = new BasicTaxon { Id = 5, ScientificName = "Other genus", Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 0 } }
        });
}
