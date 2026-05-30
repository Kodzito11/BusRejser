#!/usr/bin/env bash
set -Eeuo pipefail

section() {
  printf '\n==> %s\n' "$1"
}

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

section "Repository"
printf 'Root: %s\n' "$ROOT_DIR"

mapfile -t solutions < <(find . -maxdepth 1 -name '*.sln' -type f | sort)
if [ "${#solutions[@]}" -eq 0 ]; then
  printf 'Error: no solution file found in repository root.\n' >&2
  exit 1
fi

if [ "${#solutions[@]}" -gt 1 ]; then
  printf 'Error: multiple solution files found; refusing to guess:\n' >&2
  printf '  %s\n' "${solutions[@]}" >&2
  exit 1
fi

solution="${solutions[0]}"
printf 'Solution: %s\n' "$solution"

section "dotnet restore"
dotnet restore "$solution"

section "dotnet build"
dotnet build "$solution" --no-restore

section "Detect active test projects"
mapfile -t solution_projects < <(
  dotnet sln "$solution" list |
    awk 'found && NF { print "./" $0 } /^-+$/ { found = 1 }' |
    sed 's#\\#/#g'
)

mapfile -t test_projects < <(
  printf '%s\n' "${solution_projects[@]}" |
    while IFS= read -r project; do
      if [ -f "$project" ] && grep -Eiq '<IsTestProject>[[:space:]]*true[[:space:]]*</IsTestProject>|Microsoft\.NET\.Test\.Sdk|xunit|NUnit|MSTest' "$project"; then
        printf '%s\n' "$project"
      fi
    done
)

if [ "${#test_projects[@]}" -eq 0 ]; then
  printf 'No active test projects found in %s; skipping dotnet test.\n' "$solution"
else
  printf 'Found %s active test project(s):\n' "${#test_projects[@]}"
  printf '  %s\n' "${test_projects[@]}"

  section "dotnet test"
  for test_project in "${test_projects[@]}"; do
    printf '\n-- %s\n' "$test_project"
    dotnet test "$test_project"
  done
fi

section "Success"
printf 'Codex validation passed.\n'
