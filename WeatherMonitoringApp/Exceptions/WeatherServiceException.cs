namespace WeatherMonitoringApp.Exceptions;

public class WeatherServiceException : Exception
{
    public int StatusCode { get; }

    public WeatherServiceException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
