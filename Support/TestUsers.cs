using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// The users one test creates. Every user is deleted through the API when the test ends, so a failed test never
/// leaves an account on the site. "Account not found" is fine (the test already deleted the user in the UI, or
/// never got as far as creating it). A failed delete is logged, never thrown, so it cannot replace the test's
/// own result.
/// </summary>
public sealed class TestUsers
{
    private readonly List<User> _users = new();
    private readonly IPlaywright _playwright;
    private readonly string _baseUrl;

    public TestUsers(IPlaywright playwright, string baseUrl)
    {
        _playwright = playwright;
        _baseUrl = baseUrl;
    }

    /// <summary>A new random user, not yet on the site (for a test that signs up through the UI).</summary>
    public User New()
    {
        var user = TestData.RandomUser();
        _users.Add(user);
        return user;
    }

    /// <summary>A new random user, already created on the site through the API.</summary>
    public async Task<User> CreateViaApiAsync()
    {
        var user = New(); // registered first, so a half-finished create is still cleaned up
        await using var api = await AccountApi.CreateAsync(_playwright, _baseUrl);
        await api.CreateAccountAsync(user);
        return user;
    }

    public async Task DeleteAllAsync()
    {
        if (_users.Count == 0) return;
        try
        {
            await using var api = await AccountApi.CreateAsync(_playwright, _baseUrl);
            foreach (var user in _users)
            {
                try
                {
                    await api.DeleteAccountAsync(user, allowMissing: true);
                }
                catch (Exception ex)
                {
                    TestContext.Out.WriteLine($"Cleanup failed, account {user.Email} may be left on the site: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Cleanup failed, could not reach the API: {ex.Message}");
        }
        finally
        {
            _users.Clear();
        }
    }
}
