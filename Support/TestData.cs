namespace IDT_Task.Support;

public record User(
    string Name, string Email, string Password,
    string FirstName, string LastName, string Company,
    string Address, string Address2, string Country,
    string State, string City, string Zipcode, string Mobile);

public static class TestData
{
    public static User RandomUser()
    {
        var id = Guid.NewGuid().ToString("N")[..10];
        return new User(
            Name: $"idt{id}",
            Email: $"idt_{id}@example.com",
            Password: "Passw0rd!" + id[..4],
            FirstName: "Test",
            LastName: "User",
            Company: "IDT",
            Address: "1 Test Street",
            Address2: "Suite 2",
            Country: "Canada",
            State: "Ontario",
            City: "Toronto",
            Zipcode: "M5V 2T6",
            Mobile: "4165550100");
    }

    // The site accepts any card values on its payment form.
    public const string CardName = "Test User";
    public const string CardNumber = "4111111111111111";
    public const string CardCvc = "123";
    public const string CardExpiryMonth = "12";
    public const string CardExpiryYear = "2030";
}
