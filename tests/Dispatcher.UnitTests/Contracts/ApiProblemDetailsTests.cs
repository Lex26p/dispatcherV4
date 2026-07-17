using Dispatcher.Contracts.Common;

namespace Dispatcher.UnitTests.Contracts;

public sealed class ApiProblemDetailsTests
{
    [Fact]
    public void ApiProblemDetails_ShouldCarryMachineCodeAndCorrelationId()
    {
        var problem = new ApiProblemDetails(
            Type: "https://dispatcher.local/problems/internal-server-error",
            Title: "Internal server error",
            Status: 500,
            Code: ApiErrorCodes.InternalServerError,
            Detail: "Failure",
            CorrelationId: "disp-test");

        Assert.Equal(ApiErrorCodes.InternalServerError, problem.Code);
        Assert.Equal("disp-test", problem.CorrelationId);
        Assert.Equal(500, problem.Status);
    }
}
