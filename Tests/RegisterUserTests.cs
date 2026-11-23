using Allure.Net.Commons.Attributes;
using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestFixture]
[AllureFeature("Account")]
public class RegisterUserTests : BaseTest
{
    [Test]
    [AllureName("Test Case 1: Register User")]
    [Description("automationexercise.com Test Case 1")]
    public async Task TC01_RegisterUser()
    {
        var user = TestData.RandomUser();

        try
        {
            await RunScenarioAsync(user);
        }
        finally
        {
            // Safety net: if the UI delete failed (or the test stopped early), remove the account through
            // the API. "Account not found" is fine: it means the UI already deleted it or it was never created.
            // A cleanup failure is logged, not thrown: throwing here would replace the scenario's own exception.
            try
            {
                await using var api = await CreateAccountApiAsync();
                await api.DeleteAccountAsync(user, allowMissing: true);
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Cleanup failed, account {user.Email} may be left on the site: {ex.Message}");
            }
        }
    }

    private async Task RunScenarioAsync(User user)
    {
        var home = await new HomePage(Page).OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        var login = await home.Header.ClickSignupLoginAsync();
        await Expect(login.NewUserSignupHeading).ToBeVisibleAsync();

        var form = await login.StartSignupAsync(user.Name, user.Email);
        await Expect(form.EnterAccountInfoHeading).ToBeVisibleAsync();

        await form.FillAccountAsync(user);
        var created = await form.SubmitAsync();
        await Expect(created.AccountCreated).ToBeVisibleAsync();

        home = await created.ContinueAsync();
        await Expect(home.Header.LoggedInAs).ToContainTextAsync(user.Name);

        // The scenario's own last step: delete through the UI (still asserted).
        var deleted = await home.Header.ClickDeleteAccountAsync();
        await Expect(deleted.AccountDeleted).ToBeVisibleAsync();
        await deleted.ContinueAsync();
    }
}
