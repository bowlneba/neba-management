namespace Neba.Web.Server;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF { get; set; }
}

