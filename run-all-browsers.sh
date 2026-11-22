#!/usr/bin/env bash
# Runs the whole suite once per browser and prints a summary table.
#   ./run-all-browsers.sh                 chromium and webkit, headless
#   ./run-all-browsers.sh webkit          only the named ones
#   HEADED=1 ./run-all-browsers.sh        watch the browsers
# Browsers: chromium (bundled; same engine as Chrome and Edge), webkit (Safari engine), firefox.
# firefox is not in the default list: Firefox 131 does not start on the macOS 27 machine this was built on
# (launch timeout). Name it explicitly to try it: ./run-all-browsers.sh firefox
# Needs a build first: dotnet build
set -u
cd "$(dirname "$0")"

browsers=("$@")
[ ${#browsers[@]} -eq 0 ] && browsers=(chromium webkit)

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
