using IDT_Task.Pages;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>Reusable page-object sequences for setup and teardown (no assertions on the scenario itself).</summary>
public static class AccountFlow
{
    public static async Task RegisterAsync(IPage page, User user)
    {
        var home = new HomePage(page);
        var login = new LoginSignupPage(page);
        var form = new SignupFormPage(page);

        await home.OpenAsync();
        await home.ClickSignupLoginAsync();
        await login.StartSignupAsync(user.Name, user.Email);
        await form.FillAccountAsync(user);
        await form.SubmitAsync();
        await form.ContinueAsync();
    }

    public static async Task LogoutAsync(IPage page) =>
        await page.Locator("header a[href='/logout']").ClickAsync();

    public static async Task DeleteAccountAsync(IPage page)
    {
        var form = new SignupFormPage(page);
        await form.ClickDeleteAccountAsync();
        await form.ContinueAsync();
    }
}
