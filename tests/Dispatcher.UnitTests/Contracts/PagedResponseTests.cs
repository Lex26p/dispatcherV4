using Dispatcher.Contracts.Common;

namespace Dispatcher.UnitTests.Contracts;

public sealed class PagedResponseTests
{
    [Fact]
    public void Empty_ShouldReturnStablePagingMetadata()
    {
        var response = PagedResponse<string>.Empty(page: 2, pageSize: 25);

        Assert.Empty(response.Items);
        Assert.Equal(2, response.Page);
        Assert.Equal(25, response.PageSize);
        Assert.Equal(0, response.TotalCount);
        Assert.Null(response.NextCursor);
    }
}
