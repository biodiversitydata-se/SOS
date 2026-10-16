using FluentAssertions;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class VocabularyValueResolverTests
{
    [Theory]
    [InlineData("sv-SE", "Uppföljning (Kemisk)")]
    [InlineData("en-GB", "Follow-up (Chemical)")]
    public void ResolveVocabularyMappedValues_ResolvesPublicTreatment(string cultureCode, string expectedValue)
    {
        var observation = new Observation
        {
            Occurrence = new Occurrence
            {
                InvasiveSpeciesTreatment = new VocabularyValue { Id = (int)InvasiveSpeciesTreatmentId.FollowUpChemical }
            }
        };
        var resolver = CreateResolver();

        resolver.ResolveVocabularyMappedValues([observation], cultureCode, forceResolve: true);

        observation.Occurrence.InvasiveSpeciesTreatment.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("sv-SE", "Uppföljning (Kemisk)")]
    [InlineData("en-GB", "Follow-up (Chemical)")]
    public void ResolveVocabularyMappedValues_ResolvesPublicTreatmentInJson(string cultureCode, string expectedValue)
    {
        var observation = JsonNode.Parse("""
            { "occurrence": { "invasiveSpeciesTreatment": { "id": 7 } } }
            """)!;
        var resolver = CreateResolver();

        resolver.ResolveVocabularyMappedValues([observation], cultureCode, forceResolve: true);

        observation["occurrence"]!["invasiveSpeciesTreatment"]!["value"]!.GetValue<string>().Should().Be(expectedValue);
    }

    private static VocabularyValueResolver CreateResolver()
    {
        var repository = new Mock<IVocabularyRepository>();
        repository.Setup(value => value.GetAllAsync()).ReturnsAsync(
        [
            new Vocabulary
            {
                Id = VocabularyId.InvasiveSpeciesTreatment,
                Localized = true,
                Values =
                [
                    new VocabularyValueInfo
                    {
                        Id = (int)InvasiveSpeciesTreatmentId.FollowUpChemical,
                        Translations =
                        [
                            new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Uppföljning (Kemisk)" },
                            new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Follow-up (Chemical)" }
                        ]
                    }
                ]
            }
        ]);

        return new VocabularyValueResolver(repository.Object, new VocabularyConfiguration { ResolveValues = true });
    }
}
