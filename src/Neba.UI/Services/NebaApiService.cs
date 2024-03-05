using System.Runtime.Serialization;
using System.Text.Json;
using ErrorOr;

namespace Neba.UI.Services;

internal abstract class NebaApiService
{
    internal const string _serviceName = "NebaApi";

    private readonly IHttpClientFactory _httpClientFactory;

    protected NebaApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected async Task<ErrorOr<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(_serviceName);

        using var response = await httpClient.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<T>(content) ??
                   throw new SerializationException($"Deserialization failed for {typeof(T)}");
        }

        var errorMessage = GetErrorMessage(content);

        return Error.Failure(description: errorMessage ?? "Failed to get error Message");
    }

    private static string? GetErrorMessage(string content)
    {
        var errorContent = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

        if (errorContent is not null && errorContent.TryGetValue("errors", out var errors))
        {
            var errorsDictionary = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(errors.ToString()!);

            return string.Join(Environment.NewLine, errorsDictionary!.SelectMany(kvp => kvp.Value));
        }

        var detail = errorContent is not null && errorContent.TryGetValue("detail", out var value)
            ? value.ToString()
            : null;

        return detail;
    }
}
