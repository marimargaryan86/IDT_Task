using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class ProductsPage : BasePage
{
    public ProductsPage(IPage page) : base(page) { }

    public ILocator AllProductsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "All Products" });
    public ILocator SearchedProductsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Searched Products" });
    public ILocator ProductCards => Page.Locator(".features_items .product-image-wrapper");
    public ILocator ProductNames => ProductCards.Locator(".productinfo p");

    // The site has no data-qa on this page: ids first, then CSS classes.
    private ILocator SearchInput => Page.Locator("#search_product");
    private ILocator SearchButton => Page.Locator("#submit_search");
    private ILocator AddToCartOnHover(ILocator card) => card.Locator(".product-overlay a.add-to-cart");
    private ILocator CartModal => Page.Locator("#cartModal");
    private ILocator ContinueShoppingButton =>
        CartModal.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });

    /// <summary>Stays on the products page, now showing the search results.</summary>
    [AllureStep("Search for {term}")]
    public async Task<ProductsPage> SearchAsync(string term)
    {
        await SearchInput.FillAsync(term);
        await SearchButton.ClickAsync();
        return this;
    }

    /// <summary>
    /// Adds the first <paramref name="count"/> products to the cart. For each one: hover the card (the add
    /// button only shows on hover), click Add to cart, then close the "Added!" pop-up with Continue Shopping.
    /// Stays on the products page.
    /// </summary>
    /// <returns>The names of the added products, in the order they were added.</returns>
    [AllureStep("Add {count} products to the cart")]
    public async Task<IReadOnlyList<string>> AddProductsToCartAsync(int count)
    {
        var added = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var card = ProductCards.Nth(i);
            added.Add((await ProductNames.Nth(i).InnerTextAsync()).Trim());
            await card.HoverAsync();
            await AddToCartOnHover(card).ClickAsync();
            await ContinueShoppingButton.ClickAsync();
            await CartModal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
        return added;
    }
}
