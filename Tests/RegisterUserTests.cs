using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestClass]
public class RegisterUserTests : BaseTest
{
    [TestMethod]
    public async Task RegisterUser_CreatesAndDeletesAccount()
    {
        var user = TestData.RandomUser();
        var home = new HomePage(Page);
        var login = new LoginSignupPage(Page);
        var form = new SignupFormPage(Page);

        await home.OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        await home.ClickSignupLoginAsync();
        await Expect(login.NewUserSignupHeading).ToBeVisibleAsync();

        await login.StartSignupAsync(user.Name, user.Email);
        await Expect(form.EnterAccountInfoHeading).ToBeVisibleAsync();

        await form.FillAccountAsync(user);
        await form.SubmitAsync();
        await Expect(form.AccountCreated).ToBeVisibleAsync();

        await form.ContinueAsync();
        await Expect(home.LoggedInAs).ToContainTextAsync(user.Name);

        await home.ClickDeleteAccountAsync();
        await Expect(form.AccountDeleted).ToBeVisibleAsync();
        await form.ContinueAsync();
    }
}
