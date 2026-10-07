using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using System.Net;
using System.Text;
using System.Xml;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosDataProvidersClientTests
{
    [Fact]
    public async Task GetDataProviderEmlAsXmlAsync_Success()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <eml:eml
                xmlns:eml=""eml://ecoinformatics.org/eml-2.1.1""
                xmlns:dc=""https://purl.org/dc/terms/""
                xmlns:xsi=""https://www.w3.org/2001/XMLSchema-instance""
                xmlns:sos=""https://sos-search.artdata.slu.se/""
                xmlns:gbif=""https://gbif.org/""
                xmlns:snd=""https://snd.gu.se/""
                xsi:schemaLocation=""eml://ecoinformatics.org/eml-2.1.1 https://rs.gbif.org/schema/eml-gbif-profile/1.1/eml.xsd""
                packageId=""38b4c89f-584c-41bb-bd8f-cd1def33e92f"" system=""https://sos-search.artdata.slu.se/""
                scope=""system"" xml:lang=""en"">
	            <dataset>
		            <identifier>38b4c89f-584c-41bb-bd8f-cd1def33e92f</identifier>
		            <alternateIdentifier>https://www.gbif.se/ipt/resource?r=artdata</alternateIdentifier>
		            <alternateIdentifier>urn:lsid:artdata.slu.se:SOS:Artportalen</alternateIdentifier>
		            <title>Artportalen (Swedish Species Observation System)</title>
		            <publisher>SLU Artdatabanken</publisher>
                </dataset>
            </eml:eml>");

        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(xml.OuterXml, Encoding.UTF8, "application/xml")
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var result = await client.GetDataProviderEmlAsXmlAsync(
            1,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        result.OuterXml.Should().Be(xml.OuterXml);
    }

    [Fact]
    public async Task GetDataProviderEmlAsXmlAsync_MissingProviderId()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = null
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var act = () => client.GetDataProviderEmlAsXmlAsync(
            0,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("dataProviderId");
    }

    [Fact]
    public async Task GetDataProviderEmlAsXmlAsync_NoContent()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = null
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var result = await client.GetDataProviderEmlAsXmlAsync(
            1,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDataProviderEmlAsJsonAsync_PreservesDatasetAndNamespaceMetadata()
    {
        var json = @"{
            ""eml:eml"": {
                ""@xmlns:eml"": ""eml://ecoinformatics.org/eml-2.1.1"",
                ""@xmlns:dc"": ""https://purl.org/dc/terms/"",
                ""@xmlns:xsi"": ""https://www.w3.org/2001/XMLSchema-instance"",
                ""@xmlns:sos"": ""https://sos-search.artdata.slu.se/"",
                ""@xmlns:gbif"": ""https://gbif.org/"",
                ""@xmlns:snd"": ""https://snd.gu.se/"",
                ""@xsi:schemaLocation"": ""eml://ecoinformatics.org/eml-2.1.1 https://rs.gbif.org/schema/eml-gbif-profile/1.1/eml.xsd"",
                ""@packageId"": ""38b4c89f-584c-41bb-bd8f-cd1def33e92f"",
                ""@system"": ""https://sos-search.artdata.slu.se/"",
                ""@scope"": ""system"",
                ""@xml:lang"": ""en"",
                ""dataset"": {
                    ""identifier"": ""38b4c89f-584c-41bb-bd8f-cd1def33e92f"",
                    ""alternateIdentifier"": [
                        ""https://www.gbif.se/ipt/resource?r=artdata"",
                        ""urn:lsid:artdata.slu.se:SOS:Artportalen""
                    ],
                    ""title"": ""Artportalen (Swedish Species Observation System)"",
                    ""publisher"": ""SLU Artdatabanken""
                }
            }
        }";

        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var result = await client.GetDataProviderEmlAsJsonAsync(
            1,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        var eml = result!["eml:eml"]!;
        eml["@xmlns:eml"]!.GetValue<string>().Should().Be("eml://ecoinformatics.org/eml-2.1.1");
        eml["@packageId"]!.GetValue<string>().Should().Be("38b4c89f-584c-41bb-bd8f-cd1def33e92f");
        var dataset = eml["dataset"]!;
        dataset["identifier"]!.GetValue<string>().Should().Be("38b4c89f-584c-41bb-bd8f-cd1def33e92f");
        dataset["title"]!.GetValue<string>().Should().Be("Artportalen (Swedish Species Observation System)");
        dataset["alternateIdentifier"]!.AsArray().Select(value => value!.GetValue<string>()).Should().Equal(
            "https://www.gbif.se/ipt/resource?r=artdata", "urn:lsid:artdata.slu.se:SOS:Artportalen");
    }
        
    [Fact]
    public async Task GetDataProviderEmlAsJsonAsync_MissingProviderId()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = null
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var act = () => client.GetDataProviderEmlAsJsonAsync(
            0,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("dataProviderId");
    }

    [Fact]
    public async Task GetDataProviderEmlAsJsonAsync_NoContent()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = null
            };
        });
        var client = new SosDataProvidersClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dataproviders.example.test/")
        });

        var result = await client.GetDataProviderEmlAsJsonAsync(
            1,
            new DataProviderEmlOptions
            {
                CultureCode = "en-GB"
            },
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
