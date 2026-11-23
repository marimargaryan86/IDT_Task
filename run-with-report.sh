#!/usr/bin/env bash
# Runs the tests, then builds and opens the Allure HTML report.
#   ./run-with-report.sh                                   all tests
#   ./run-with-report.sh --filter "Name=TC09_SearchProduct"
#   NO_OPEN=1 ./run-with-report.sh                         build the report but do not open a browser
# Old results are deleted first, so the report shows only this run.
# Needs the Allure command-line tool (brew install allure) and Java.
set -u
cd "$(dirname "$0")"

REPORT=allure-report

# Allure writes next to the built dll: bin/<configuration>/<framework>/allure-results. Rather than hard-code
# the configuration (Debug/Release) and framework (net10.0), delete every old results folder before the run
# and use the one the run creates.
rm -rf bin/*/*/allure-results "$REPORT"

dotnet test "$@"
status=$?

RESULTS=$(ls -d bin/*/*/allure-results 2>/dev/null | head -1)
if [ -z "$RESULTS" ] || [ -z "$(ls -A "$RESULTS" 2>/dev/null)" ]; then
  echo "No Allure results were written under bin/*/*/allure-results (did the tests start?)." >&2
  exit "${status:-1}"
fi

allure generate "$RESULTS" -o "$REPORT" --clean
echo "Report written to $REPORT/index.html"

if [ -z "${NO_OPEN:-}" ]; then
  allure open "$REPORT"   # starts a local web server and opens your browser; stop it with Ctrl-C
fi

exit "$status"
