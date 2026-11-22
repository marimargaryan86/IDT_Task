#!/usr/bin/env bash
# Runs the tests, then builds and opens the Allure HTML report.
#   ./run-with-report.sh                                   all tests
#   ./run-with-report.sh --filter "Name=TC09_SearchProduct"
#   NO_OPEN=1 ./run-with-report.sh                         build the report but do not open a browser
# Old results are deleted first, so the report shows only this run.
# Needs the Allure command-line tool (brew install allure) and Java.
set -u
cd "$(dirname "$0")"

RESULTS=bin/Debug/net9.0/allure-results
REPORT=allure-report

rm -rf "$RESULTS" "$REPORT"

dotnet test "$@"
status=$?

if [ ! -d "$RESULTS" ] || [ -z "$(ls -A "$RESULTS" 2>/dev/null)" ]; then
  echo "No Allure results were written to $RESULTS (did the tests start?)." >&2
  exit "${status:-1}"
fi

allure generate "$RESULTS" -o "$REPORT" --clean
echo "Report written to $REPORT/index.html"

if [ -z "${NO_OPEN:-}" ]; then
  allure open "$REPORT"   # starts a local web server and opens your browser; stop it with Ctrl-C
fi

exit "$status"
