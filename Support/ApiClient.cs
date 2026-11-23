using System.Text.Json;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Base for the clients of the site's API (<see cref="AccountApi"/>, <see cref="ProductApi"/>). The API answers
/// HTTP 200 with a JSON body that carries its own "responseCode". It is used for test setup, cleanup and expected
/// data, never for the UI assertions themselves.
/// </summary>
public abstract class ApiClient : IAsyncDisposable
{
    protected ApiClient(IAPIRequestContext api) => Api = api;

    protected IAPIRequestContext Api { get; }

    protected static Task<IAPIRequestContext> NewContextAsync(IPlaywright playwright, string baseUrl) =>
        playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });

    /// <summary>
    /// Sends the call, checks that the JSON "responseCode" is one of <paramref name="expectedCodes"/> and returns
    /// the JSON body. An HTTP 5xx (the site is overloaded) or a body that is not the usual JSON fails with a message
    /// that says what came back.
    /// </summary>
    protected static async Task<JsonElement> SendAsync(string what, int[] expectedCodes, Func<Task<IAPIResponse>> call)
    {
        var response = await call();
        var body = await response.TextAsync();
        if (response.Status >= 500)
            throw new InvalidOperationException($"API {what}: HTTP {response.Status}, body starts with: {Snippet(body)}");

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
            throw new InvalidOperationException(
                $"API {what}: unexpected response, HTTP {response.Status}, body starts with: {Snippet(body)}", ex);
        }

        return expectedCodes.Contains(code)
            ? root
            : throw new InvalidOperationException($"API {what} returned responseCode {code}: {Snippet(body)}");
    }

    /// <summary>The first 200 characters of a response body, on one line, for error messages.</summary>
    private static string Snippet(string body)
    {
        var oneLine = string.Join(' ', body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return oneLine.Length <= 200 ? $"\"{oneLine}\"" : $"\"{oneLine[..200]}...\"";
    }

    public async ValueTask DisposeAsync() => await Api.DisposeAsync();
}
