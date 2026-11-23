# IDT_Task

[![Tests](https://github.com/marimargaryan86/IDT_Task/actions/workflows/tests.yml/badge.svg)](https://github.com/marimargaryan86/IDT_Task/actions/workflows/tests.yml)

Playwright (C#) + NUnit suite for http://automationexercise.com, built with the Page Object Model.

## Quick start
Needs the .NET 10 SDK.
```bash
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium   # one time
dotnet test
```
No `pwsh`? Use the Node that Playwright bundles (macOS Apple silicon; other systems: the matching `node/<platform>`
folder): `cd bin/Debug/net10.0/.playwright && ./node/darwin-arm64/node package/cli.js install chromium && cd -`

## Scenarios
The four assigned cases from https://automationexercise.com/test_cases, one test each, following the site's steps.
In the Allure report each test is named after its case.

| Case | Title | Test |
|------|-------|------|
| 1 | Register User | `RegisterUserTests.TC01_RegisterUser` |
| 9 | Search Product | `SearchProductTests.TC09_SearchProduct` |
| 16 | Place Order: Login before Checkout | `PlaceOrderLoginBeforeCheckoutTests.TC16_PlaceOrder_LoginBeforeCheckout` |
| 25 | Verify Scroll Up using 'Arrow' button and Scroll Down functionality | `ScrollTests.TC25_ScrollUpUsingArrow_AndScrollDown` |

- **Case 9** compares the page with the site's own search API (`/api/searchProduct`): exactly the products the API
  returns, in the same order, all visible. The site matches on category as well as name, so the term alone cannot
  say which products belong.
- **Case 16** needs an existing account, so the test creates one through the API first.
- **Test users** are random and per test (`TestUsers`). Cases 1 and 16 end by deleting the account in the UI; after
  every test any user still left is deleted through the API, so a failed test leaves no account behind.

## Structure
- `Pages/`: page objects. Locators are properties at the top (`data-qa` first where the site has it); actions that
  lead to another page return that page.
- `Components/`: `HeaderComponent`, `FooterComponent`, shared by every page through `BasePage`.
- `Tests/`: steps and assertions.
- `Support/`: `BaseTest` (setup and teardown) and its helpers `RequestRouting` (ad blocking, page-load retry),
  `FailureEvidence` (trace, screenshot, log) and `TestUsers`; `Settings`, `TestData`, `Routes`; the API layer
  (`ApiClient` with retry on 5xx, `AccountApi`, `ProductApi`).

## Run
```bash
dotnet test                                      # headless, one test class after another
HEADED=1 dotnet test                             # watch it run
dotnet test --filter "Name=TC09_SearchProduct"   # one test
BROWSER=webkit dotnet test                       # chromium (default), webkit or firefox
dotnet test -- NUnit.NumberOfTestWorkers=4       # test classes in parallel
./run-all-browsers.sh                            # every browser, with a summary table
```
Chromium is the engine of Chrome and Edge; WebKit is Safari's engine (Playwright cannot drive the Safari app).
Install WebKit and Firefox once the same way as Chromium (`install chromium webkit firefox`).

## Configuration
`.runsettings` is the single place for values (browser, headless, timeouts, parallel workers, base URL, retries); each
has a comment there. `dotnet test` picks it up through the csproj; in an IDE, select it as the run settings file.
An environment variable overrides a value for one run: `BROWSER`, `HEADED=1`, `IDT_BASE_URL`,
`IDT_ACTION_TIMEOUT_MS`, `IDT_RETRY_ATTEMPTS`. Values have no defaults in code: a missing one stops the run with a
message naming it. The viewport (1440 x 900) and ad blocking are fixed in the code. There are no secrets here (public
site, random users); if one is ever needed, read it from an environment variable, never from `.runsettings`.
```bash
IDT_BASE_URL=https://staging.example.com dotnet test
dotnet test --settings other.runsettings
```

## Reporting (Allure)
Results go to `bin/Debug/net10.0/allure-results`. `./run-with-report.sh` runs the tests and opens the report
(`NO_OPEN=1` only builds it; needs `brew install allure` and Java).
- **Steps:** page-object actions and API calls carry `[AllureStep("...")]`; use `{parameterName}` in the title to show
  a value and `[AllureParameter(Ignore = true)]` on anything sensitive. `Expect(...)` assertions are not steps.
- **Failures:** screenshot, Playwright trace (open with `show-trace`) and console output (URL, HTTP and console
  errors) next to the failed step. Test classes are grouped with `[AllureFeature]`.
- **Colours:** a failed `Expect(...)` makes the test yellow ("broken"), not red, because Allure reserves "failed" for
  NUnit asserts. Treat both as failures.

## Continuous integration
`.github/workflows/tests.yml` builds and runs the tests on every push to `main`, every pull request and on demand
(Actions tab, **Tests**, **Run workflow**, with a browser choice). It uploads the Allure results and the failure
screenshots and traces as the `test-evidence` artifact, also when tests fail. The Allure report of a run on `main` is
published at https://marimargaryan86.github.io/IDT_Task/ (one-time setup: Settings, Pages, Source: GitHub Actions).

## Known limitations
- **The site is not always available.** It is a public practice site that sometimes answers HTTP 503, more often
  under load (seen on pages, search and product pictures). The output then shows an `HTTP 503` line next to the
  failure screenshot and trace. A site hiccup passes on a re-run; a real defect fails the same way every time.
- **Retried on 5xx:** GET page loads of the site, up to `RetryAttempts` times (2); the browser only gets the final
  answer. Every retry is logged ("RETRY GET ...") in the test output and the Allure Console Output, also when the
  test passes. `IDT_RETRY_ATTEMPTS=0` turns it off. The site's API calls are retried too (`ApiClient`).
- **Never retried:** forms (POST: sign-up, login, payment), scripts, images, assertions and whole tests, so a wrong
  result fails at once.
- **Ads:** ad and tracker requests are blocked first. No run has shown an ad covering the page; if one ever blocks a
  click, the failure screenshot and trace show it, and it can be handled then with the real ad to test against.
