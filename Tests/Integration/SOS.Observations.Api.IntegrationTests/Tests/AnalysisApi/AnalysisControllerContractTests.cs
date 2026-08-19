using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOS.Analysis.Api.Controllers;
using System.Reflection;

namespace SOS.Observations.Api.IntegrationTests.Tests.AnalysisApi;

public class AnalysisControllerContractTests
{
    [Theory]
    [InlineData(nameof(AnalysisController.OrderAooAndEooInternalAsync))]
    [InlineData(nameof(AnalysisController.OrderAooAndEooArticle17InternalAsync))]
    public void OrderEndpoint_DeclaresStringResponseForOk(string methodName)
    {
        var method = typeof(AnalysisController).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);

        method.Should().NotBeNull();
        var responseType = method!
            .GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Single(attribute => attribute.StatusCode == StatusCodes.Status200OK);

        responseType.Type.Should().Be(typeof(string));
    }
}
