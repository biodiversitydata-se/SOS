using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SOS.ElasticSearch.Proxy.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace SOS.ElasticSearch.Proxy.UnitTests.Extensions
{
    public class ElasticSearchRequestExtensionsTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void UpdateSort_GivenOneSortField()
        {
            // Arrange
            string body = """
                {"size":1,"query":{"match_all":{}},"from":0,"sort":[{"_id":{"order":"asc"}}]}
                """;
            var bodyDictionary = (IDictionary<string, Object>)JsonConvert.DeserializeObject<ExpandoObject>(body, new ExpandoObjectConverter())!;

            // Act
            ElasticSearchRequestExtensions.UpdateSort(bodyDictionary);
            string strResult = System.Text.Json.JsonSerializer.Serialize(bodyDictionary);

            // Assert
            string expected = """
                {"size":1,"query":{"match_all":{}},"from":0,"sort":[{"event.endDate":{"order":"desc"}}]}
                """;
            strResult.Should().Be(expected);                        
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdateSort_GivenMultipleSortFields()
        {
            // Arrange
            string body = """
                {"size":1,"query":{"match_all":{}},"from":0,"sort":[{"_id":{"order":"asc"}},{"occurrenceId":{"order":"desc"}}]}
                """;
            var bodyDictionary = (IDictionary<string, Object>)JsonConvert.DeserializeObject<ExpandoObject>(body, new ExpandoObjectConverter())!;

            // Act
            ElasticSearchRequestExtensions.UpdateSort(bodyDictionary);
            string strResult = System.Text.Json.JsonSerializer.Serialize(bodyDictionary);

            // Assert
            string expected = """
                {"size":1,"query":{"match_all":{}},"from":0,"sort":[{"event.endDate":{"order":"desc"}}]}
                """;
            strResult.Should().Be(expected);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdateSort_GivenNoSortFields()
        {
            // Arrange
            string body = """
                {"size":1,"query":{"match_all":{}},"from":0}
                """;
            var bodyDictionary = (IDictionary<string, Object>)JsonConvert.DeserializeObject<ExpandoObject>(body, new ExpandoObjectConverter())!;

            // Act
            ElasticSearchRequestExtensions.UpdateSort(bodyDictionary);
            string strResult = System.Text.Json.JsonSerializer.Serialize(bodyDictionary);

            // Assert
            string expected = """
                {"size":1,"query":{"match_all":{}},"from":0}
                """;
            strResult.Should().Be(expected);
        }
    }
}