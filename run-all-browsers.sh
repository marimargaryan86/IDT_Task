#!/usr/bin/env bash
# Runs the whole suite once per browser and prints a summary table.
#   ./run-all-browsers.sh                 chromium, webkit and firefox, headless
#   ./run-all-browsers.sh webkit          only the named ones
#   HEADED=1 ./run-all-browsers.sh        watch the browsers
# Browsers: chromium (bundled; same engine as Chrome and Edge), webkit (Safari engine), firefox.
# Needs a build and the browsers first: dotnet build, then the install command in README.md (Browsers).
set -u
cd "$(dirname "$0")"

browsers=("$@")
[ ${#browsers[@]} -eq 0 ] && browsers=(chromium webkit firefox)

results=()
for b in "${browsers[@]}"; do
  echo "=== $b"   
  case "$b" in
    chromium|firefox|webkit) out=$(BROWSER="$b" dotnet test --no-build 2>&1) ;;
    *) echo "unknown browser: $b"; continue ;;
  esac
  summary=$(echo "$out" | grep -E "Passed!|Failed!" | tail -1 | sed -E 's/^ *//')
  [ -z "$summary" ] && summary="no result (see the error below)\n$(echo "$out" | grep -E "Error Message|Exception|error" | head -3)"
  echo -e "$summary"
  results+=("$(printf '%-9s %s' "$b" "$(echo -e "$summary" | head -1)")")
done

echo
echo "Summary"
printf '%s\n' "${results[@]}"
