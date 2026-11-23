using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>Reads products through the site's API, as the expected data for UI checks.</summary>
public sealed class ProductApi : ApiClient
{
    private ProductApi(IAPIRequestContext api) : base(api) { }

    public static async Task<ProductApi> CreateAsync(IPlaywright playwright, string baseUrl) =>
        new(await NewContextAsync(playwright, baseUrl));

    /// <summary>
    /// Names of the products the site's search API returns for <paramref name="term"/>, in the API's order
    /// (the same order the search page lists them in).
    /// </summary>
    [AllureStep("API: search products for {term}")]
    public async Task<IReadOnlyList<string>> SearchProductNamesAsync(string term)
    {
        var form = Api.CreateFormData().Set("search_product", term);
        var body = await SendAsync("search products", new[] { 200 },
            () => Api.PostAsync(Routes.SearchProductApi, new() { Form = form }));

        return body.GetProperty("products").EnumerateArray()
            .Select(p => p.GetProperty("name").GetString()!.Trim())
            .ToList();
    }
}
