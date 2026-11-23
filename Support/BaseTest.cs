using Allure.NUnit;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace IDT_Task.Support;

/// <summary>
/// Setup and teardown for every test; the work is done by <see cref="AdBlocking"/>, <see cref="FailureEvidence"/>
/// (trace, screenshot, log) and <see cref="TestUsers"/> (users the test created, deleted afterwards). [AllureNUnit] here applies to every class that inherits it.
/// </summary>
[AllureNUnit]
public abstract class BaseTest : PageTest
{
    private Settings? _config;
    private FailureEvidence? _evidence;
    private TestUsers? _users;

    protected Settings Config => _config ??= Settings.Load();

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = Config.BaseUrl,
        ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
    };

    /// <summary>A new random user, not yet on the site (for a test that signs up through the UI).</summary>
    protected User NewTestUser() => _users!.New();

    /// <summary>A new random user, already created on the site through the API.</summary>
    protected Task<User> CreateTestUserViaApiAsync() => _users!.CreateViaApiAsync();

    /// <summary>An API client for reading products, pointed at the configured site.</summary>
    protected Task<ProductApi> CreateProductApiAsync() => ProductApi.CreateAsync(Playwright, Config.BaseUrl);

    [SetUp]
    public async Task SetUpPage()
    {
        // One test class instance serves all its tests, so every test gets its own helpers.
        _evidence = new FailureEvidence();
        _users = new TestUsers(Playwright, Config.BaseUrl);

        Page.SetDefaultTimeout(Config.ActionTimeoutMs);
        Page.SetDefaultNavigationTimeout(Config.ActionTimeoutMs);

        await _evidence.StartAsync(Context, Page);
        await AdBlocking.InstallAsync(Context);
    }

    /// <summary>One teardown, so the order is fixed: evidence first (the page still shows the failure), then the
    /// test users are removed, even if collecting evidence failed.</summary>
    [TearDown]
    public async Task TearDownTest()
    {
        try
        {
            if (_evidence is not null) await _evidence.CollectAsync(Context, Page);
        }
        finally
        {
            if (_users is not null) await _users.DeleteAllAsync();
        }
    }
}
