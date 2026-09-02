using FluentAssertions;
using Microsoft.Extensions.Logging;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Models.TaxonTree;

public class TaxonTreeCycleTests
{
    [Fact]
    public void CyclicTaxonRelationsProduceCycleDiagnosticsAndNoTopologicalSort()
    {
        var taxa = new Dictionary<int, IBasicTaxon>
        {
            [1] = new BasicTaxon
            {
                Id = 1,
                ScientificName = "Taxon one",
                Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 2 }
            },
            [2] = new BasicTaxon
            {
                Id = 2,
                ScientificName = "Taxon two",
                Attributes = new TaxonAttributes { ParentDyntaxaTaxonId = 1 }
            }
        };

        var logger = new RecordingLogger();
        var tree = TaxonTreeFactory.CreateTaxonTree(taxa, logger);
        var cycles = TaxonTreeCyclesDetectionHelper.CheckForCycles(tree);

        tree.ReverseTopologicalSortById.Should().BeEmpty();
        cycles.Should().ContainSingle();
        cycles.Single().Select(vertex => vertex.Value).Should().BeEquivalentTo(new[] { 1, 2 });
        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Error);
        logger.Entries[0].Exception.Should().NotBeNull();
        logger.Entries[0].Message.Should().Contain("Cycle component taxon IDs: 1, 2");
        logger.Entries[0].Message.Should().Contain("Main: 2 (Taxon two) -> 1 (Taxon one)");
        logger.Entries[0].Message.Should().Contain("Main: 1 (Taxon one) -> 2 (Taxon two)");
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }
    }
}
