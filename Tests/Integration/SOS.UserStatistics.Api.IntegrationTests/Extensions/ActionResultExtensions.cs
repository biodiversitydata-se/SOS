namespace SOS.UserStatistics.Api.IntegrationTests.Extensions;

public static class ResultExtensions
{
    public static T GetResultObject<T>(this HttpResponseMessage responseMessage)
    {
        var okObjectResult = (OkObjectResult)(Results.Ok(responseMessage.Content));
        return (T)okObjectResult.Value;
    }
}