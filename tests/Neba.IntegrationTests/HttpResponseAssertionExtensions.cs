using System.Net.Http.Json;
using System.Text.Json;

namespace Neba.IntegrationTests;

internal static class HttpResponseAssertionExtensions
{
#pragma warning disable S2325 // Extension methods should be static
    extension(HttpResponseMessage response)
    {
        public async Task ShouldBeNotFound(string title, string detail, Dictionary<string, object>? metadata = null)
        {
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);

            JsonElement payload = await response.Content.ReadFromJsonAsync<JsonElement>();

            payload.GetProperty("title").GetString().ShouldBe(title);
            payload.GetProperty("detail").GetString().ShouldBe(detail);

            foreach (KeyValuePair<string, object> keyValuePair in metadata ?? [])
            {
                payload.TryGetProperty(keyValuePair.Key, out JsonElement prop).ShouldBeTrue();
                prop.GetRawText().ShouldBe(JsonSerializer.Serialize(keyValuePair.Value));
            }
        }
    }
}
