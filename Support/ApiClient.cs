using System.Text.Json;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Base for the clients of the site's API (<see cref="AccountApi"/>, <see cref="ProductApi"/>). The API answers
/// HTTP 200 with a JSON body that carries its own "responseCode". A 5xx means the overloaded server refused the
/// call, so it is retried. The API is only used for test setup, cleanup and expected data, never for the UI
/// assertions themselves, so a retry never hides a UI failure.
/// </summary>
public abstract class ApiClient : IAsyncDisposable
{
    private const int Attempts = 4;

    protected ApiClient(IAPIRequestContext api) => Api = api;

    protected IAPIRequestContext Api { get; }

    protected static Task<IAPIRequestContext> NewContextAsync(IPlaywright playwright, string baseUrl) =>
        playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });

    /// <summary>
    /// Sends the call (retrying on 5xx), checks that the JSON "responseCode" is one of
    /// <paramref name="expectedCodes"/> and returns the JSON body.
    /// </summary>
    protected static async Task<JsonElement> SendAsync(string what, int[] expectedCodes, Func<Task<IAPIResponse>> call)
    {
        string last = "no response";
        for (var attempt = 1; attempt <= Attempts; attempt++)
        {
            var response = await call();
            if (response.Status >= 500)
            {
                last = $"HTTP {response.Status}";
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt)); // back off before the next attempt
                continue;
            }

            var body = await response.TextAsync();
            JsonElement root;
            int code;
            try
            {
                using var json = JsonDocument.Parse(body);
                root = json.RootElement.Clone(); // outlives the JsonDocument
                code = root.GetProperty("responseCode").GetInt32();
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                // Not the JSON the API normally returns (for example an HTML error page): say what came back.
                throw new InvalidOperationException(
                    $"API {what}: unexpected response, HTTP {response.Status}, body starts with: {Snippet(body)}", ex);
            }

            if (expectedCodes.Contains(code)) return root;
            throw new InvalidOperationException($"API {what} returned responseCode {code}: {Snippet(body)}");
        }
        throw new InvalidOperationException($"API {what} failed after {Attempts} attempts ({last}).");
    }

    /// <summary>The first 200 characters of a response body, on one line, for error messages.</summary>
    private static string Snippet(string body)
    {
        var oneLine = string.Join(' ', body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return oneLine.Length <= 200 ? $"\"{oneLine}\"" : $"\"{oneLine[..200]}...\"";
    }

    public async ValueTask DisposeAsync() => await Api.DisposeAsync();
}
