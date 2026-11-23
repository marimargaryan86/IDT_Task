# IDT_Task

Playwright (C#) + NUnit suite for http://automationexercise.com, built with the Page Object Model.

## Quick start
Needs the .NET 10 SDK.
```bash
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium   # one time; no pwsh? see "Run" below
dotnet test
```
All four tests run headless, one after another. Other ways to run them (one test, watch the browser, parallel,
another browser, Allure report) are in the sections below.

## Continuous integration
`.github/workflows/tests.yml` builds the project and runs the tests on every push to `main`, on every pull
request, and on demand from the Actions tab. It uploads the Allure results and the failure screenshots and
traces as the `test-evidence` artifact, also when the tests fail. A red run is not always a code problem: see
[Known limitations](#known-limitations).

**Run it by hand:** Actions tab, then **Tests**, then **Run workflow**. Choose the branch and, if you want, the
browser (chromium, webkit or firefox) and the number of parallel workers (`1` = sequential, the default). Pushes
and pull requests always run Chromium, sequentially.

**Read the result:** open the run. Green means every test passed. After a failure, download the `test-evidence`
artifact for the screenshots and traces. The Allure report of a run on `main` is published at
https://marimargaryan86.github.io/IDT_Task/ (also linked as **View deployment** on the run page). One-time setup
for that: repository Settings, Pages, Source: GitHub Actions.

## Known limitations
- **The site is not always available.** automationexercise.com is a public practice site that sometimes answers
  HTTP 503 (seen on `delete_account`, on search and on product pictures), more often under load. The test then
  times out waiting for a page, and the output shows an `HTTP 503` line next to the failure screenshot and trace.
- **How to tell it from a real defect:** re-run. A site hiccup passes on the next run; a real defect fails the
  same way every time, at the same step. The run on GitHub can be repeated with **Re-run failed jobs**.
- **What is retried:** only the API calls that create and delete test users (`Support/AccountApi.cs`, on 5xx).
  Page loads and assertions are never retried, so a failing assertion is never hidden.
- **Firefox does not start** on the macOS 27 machine this was built on (see [Browsers](#browsers)).

## Scenarios
The four assigned cases from https://automationexercise.com/test_cases, one test each, following the site's
steps. In the Allure report each test is named after its case.

| Case | Title | Test |
|------|-------|------|
| 1 | Register User | `RegisterUserTests.TC01_RegisterUser` |
| 9 | Search Product | `SearchProductTests.TC09_SearchProduct` |
| 16 | Place Order: Login before Checkout | `PlaceOrderLoginBeforeCheckoutTests.TC16_PlaceOrder_LoginBeforeCheckout` |
| 25 | Verify Scroll Up using 'Arrow' button and Scroll Down functionality | `ScrollTests.TC25_ScrollUpUsingArrow_AndScrollDown` |

Case 16 needs an existing account: the test creates one through the site's API first, runs the case (which
ends by deleting the account in the UI) and deletes it through the API only if the UI step did not get there.

## Structure
- `Pages/`: page objects. Locators are properties at the top (data-qa first where the site has it);
  actions that lead to another page return that page.
- `Components/`: `HeaderComponent`, `FooterComponent`, shared by every page through `BasePage`.
- `Tests/`: steps and assertions.
- `Support/`: `BaseTest` (setup, ad blocking, failure evidence), `Settings` (configuration), `TestData`,
  `Routes`, `AccountApi` (create/delete users through the site's API, with retry on 5xx).

### Ads
The site shows ads that can cover the page. `BaseTest` blocks ad and tracker requests first (turn off with
`IDT_BLOCK_ADS=false`). There is no other ad handling: no run on the real site has shown an ad covering the
page. If one ever blocks a click, the failure screenshot and trace show it; handle it then, with the real ad to
test against.

## Run
```bash
dotnet build

dotnet test                                   # uses .runsettings (headless, sequential)
HEADED=1 dotnet test                          # watch it run
dotnet test --filter "Name=TC09_SearchProduct"   # one test
```

One-time browser install, after `dotnet build`. Any OS, with PowerShell (`pwsh`):
```bash
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```
macOS (Apple silicon) without PowerShell, using the Node bundled with Playwright:
```bash
cd bin/Debug/net10.0/.playwright && ./node/darwin-arm64/node package/cli.js install chromium && cd -
```
On other machines without PowerShell, use the `node/<platform>` folder that matches yours (for example
`linux-x64` or `win32_x64`).
Failure screenshots and traces go to `TestResults/` (next to the built dll, under `bin/Debug/net10.0/`).
Open a trace with `show-trace` from the same bundled CLI.

## Parallel execution
By default the tests run one after another. Parallel execution is switched on by raising the number of workers:
then test classes run at the same time and the tests inside one class still run one after another
(`Support/AssemblyConfig.cs`). Each test creates its own random user and its own browser context, so they do
not share data. The number of parallel workers is `NumberOfTestWorkers` in `.runsettings` (default `1` =
sequential). Override it for one run, or change the value in the file to make it permanent:
```bash
dotnet test -- NUnit.NumberOfTestWorkers=4
```
With 4 workers the run takes about as long as the slowest test (Place Order), instead of the sum of all
four. A busy moment on the site can fail a test in either mode (see
[Known limitations](#known-limitations)); read the failure evidence before blaming parallelism. Tests of one class never overlap
because `BaseTest` keeps per-test state in instance fields.

## Reporting (Allure)
Every run writes Allure results to `bin/Debug/net10.0/allure-results` (set in `allureConfig.json`). Build and
open the HTML report with:
```bash
./run-with-report.sh                      # run all tests, then open the report
NO_OPEN=1 ./run-with-report.sh            # build the report only (allure-report/index.html)
./run-with-report.sh --filter "Name=TC09_SearchProduct"
```
It needs the Allure command-line tool and Java (`brew install allure`). It deletes old results first, so the
report shows only the latest run. To report on an existing results folder:
`allure generate bin/Debug/net10.0/allure-results -o allure-report --clean`.

- **Steps:** every page-object action and the API setup calls are Allure steps, added with
  `[AllureStep("...")]` (`Allure.Net.Commons.Attributes`). Put the attribute on a new page method to make it a
  step. Use `{parameterName}` in the title to show a value, and `[AllureParameter(Ignore = true)]` on anything
  sensitive (the `User` and password parameters are hidden). `Expect(...)` assertions are not steps.
- **Failures:** the report shows the screenshot, the Playwright trace and the console output (URL, HTTP errors,
  console errors) next to the failed step. Open a trace with `show-trace` from the bundled CLI.
- **Grouping:** test classes carry `[AllureFeature("...")]` (Account, Catalog, Checkout, Navigation).
- **Status colours:** a failing step is red ("failed"), but the test itself is yellow ("broken") when Playwright
  throws, which includes failed `Expect(...)` assertions. Allure reserves "failed" for NUnit asserts. Treat
  both as failures.

## Browsers
| Browser | Command | Status (macOS 27, Playwright 1.48) |
|---------|---------|------------------------------------|
| Chromium (bundled; the engine of Chrome and Edge) | `dotnet test` | works |
| WebKit (the engine Safari is built on) | `BROWSER=webkit dotnet test` | works |
| Firefox | `BROWSER=firefox dotnet test` | **does not start**: launch times out after 60 s |

Playwright cannot drive the real Safari app. WebKit is its own patched build of Safari's engine, the closest
equivalent. Chrome and Edge are not run separately: they use the same engine as the bundled Chromium.

Install the browsers once (WebKit and Firefox are about 230 MB each):
```bash
cd bin/Debug/net10.0/.playwright && ./node/darwin-arm64/node package/cli.js install chromium webkit firefox && cd -
```
Run several browsers and print a summary table:
```bash
./run-all-browsers.sh                  # chromium and webkit
./run-all-browsers.sh webkit firefox   # only the named ones
```
Site 503s can fail a random test in any browser (see [Known limitations](#known-limitations)). Read the failure
output (screenshot, trace and console log in the Allure report) before blaming a browser.

## Configuration
`dotnet test` reads `.runsettings` automatically (set in the csproj). **`.runsettings` is the single place
for every value**; an environment variable overrides a value for one run. The project's own values
(`TestRunParameters`, read by `Support/Settings.cs`) have no defaults in code: if one is missing from both, the
run stops at start-up with a message naming the setting. In an IDE, select `.runsettings` as the run settings
file if it does not pick it up from the csproj.

| What | `.runsettings` | Environment variable | Value in `.runsettings` |
|------|----------------|----------------------|-------------------------|
| Browser (chromium / firefox / webkit, see Browsers) | `<Playwright><BrowserName>` | `BROWSER` | chromium |
| Browser launch timeout (ms) | `<LaunchOptions><Timeout>` | | 60000 |
| Headless | `<LaunchOptions><Headless>` | `HEADED=1` shows the browser | true |
| Slow motion (ms) | `<LaunchOptions><SlowMo>` | | 0 |
| Expect timeout (ms) | `<Playwright><ExpectTimeout>` | | 10000 |
| Base URL | `BaseUrl` | `IDT_BASE_URL` | https://automationexercise.com |
| Viewport | `ViewportWidth`, `ViewportHeight` | `IDT_VIEWPORT_WIDTH`, `IDT_VIEWPORT_HEIGHT` | 1440 x 900 |
| Action / navigation timeout (ms) | `ActionTimeoutMs` | `IDT_ACTION_TIMEOUT_MS` | 30000 |
| Block ad requests | `BlockAds` | `IDT_BLOCK_ADS` | true |

Examples:
```bash
IDT_BASE_URL=https://staging.example.com dotnet test
dotnet test --settings other.runsettings        # a different config file
```
There are no real secrets in this project (public site, random users). If one is ever needed, read it from an
environment variable only and never put it in `.runsettings`.
