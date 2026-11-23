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
        // The test signs this user up through the UI. BaseTest deletes it afterwards if the UI delete at the end
        // of the case did not happen.
        var user = NewTestUser();

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
