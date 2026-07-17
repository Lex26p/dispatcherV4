namespace Dispatcher.Web.Services;

public sealed class DispatcherApiException : Exception
{
    public DispatcherApiException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
