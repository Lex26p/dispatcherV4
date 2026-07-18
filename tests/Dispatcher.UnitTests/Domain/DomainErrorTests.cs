using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Domain;

public sealed class DomainErrorTests
{
    [Fact]
    public void Create_trims_code_message_and_target()
    {
        var error = DomainError.Create(" validation.required ", " Name is required ", " name ");

        Assert.Equal("validation.required", error.Code);
        Assert.Equal("Name is required", error.Message);
        Assert.Equal("name", error.Target);
    }

    [Fact]
    public void Create_rejects_empty_code()
    {
        Assert.Throws<ArgumentException>(() => DomainError.Create(" ", "Message"));
    }
}
