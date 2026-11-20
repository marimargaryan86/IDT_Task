# IDT_Task

Playwright (C#) + MSTest suite for http://automationexercise.com, built with the Page Object Model.

## Scenarios
| Test | Class |
|------|-------|
| Register User | `Tests/RegisterUserTests` |
| Search Product | `Tests/SearchProductTests` |
| Place Order: Login before Checkout | `Tests/PlaceOrderLoginBeforeCheckoutTests` |
| Verify Scroll Up without 'Arrow' button, Scroll Down | `Tests/ScrollTests` |

## Structure
- `Pages/`: page objects (locators and actions only, no assertions)
- `Tests/`: steps and assertions
- `Support/`: `BaseTest` (base URL, ad blocking, screenshot on failure), `TestData` (random user), `AccountFlow` (register / logout / delete)

## Run
```bash
dotnet build
# one-time browser install (no PowerShell needed):
cd bin/Debug/net9.0/.playwright && ./node/darwin-arm64/node package/cli.js install chromium && cd -

dotnet test                                   # headless
HEADED=1 dotnet test                          # watch it run
HEADED=1 dotnet test --filter "Name=ScrollDown_ShowsSubscription"   # one test
```
Failure screenshots go to `TestResults/screenshots/`.
