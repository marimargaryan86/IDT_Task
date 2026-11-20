using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class LoginSignupPage : BasePage
{
    public LoginSignupPage(IPage page) : base(page) { }

    public ILocator NewUserSignupHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "New User Signup!" });
    public ILocator LoginHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Login to your account" });
    public ILocator LoginError => Page.GetByText("Your email or password is incorrect!");

    private ILocator SignupName => Page.Locator("[data-qa='signup-name']");
    private ILocator SignupEmail => Page.Locator("[data-qa='signup-email']");
    private ILocator SignupButton => Page.Locator("[data-qa='signup-button']");
    private ILocator LoginEmail => Page.Locator("[data-qa='login-email']");
    private ILocator LoginPassword => Page.Locator("[data-qa='login-password']");
    private ILocator LoginButton => Page.Locator("[data-qa='login-button']");

    public async Task StartSignupAsync(string name, string email)
    {
        await SignupName.FillAsync(name);
        await SignupEmail.FillAsync(email);
        await SignupButton.ClickAsync();
    }

    public async Task LoginAsync(string email, string password)
    {
        await LoginEmail.FillAsync(email);
        await LoginPassword.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
