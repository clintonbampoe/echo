#!/usr/bin/env bash
# Fails when a relative link in a markdown file points at a file that is not there.
set -euo pipefail
cd "$(dirname "$0")/.."

bad=0
while IFS= read -r file; do
  dir=$(dirname "$file")
  while IFS= read -r link; do
    target=${link%%#*}
    [[ -n $target ]] || continue
    [[ -e $dir/$target ]] || {
      echo "$file: broken link $link"
      bad=1
    }
  done < <(grep -oE '\]\([^)[:space:]]+\)' "$file" | sed -E 's/^\]\(//; s/\)$//' |
    grep -vE '^(https?:|mailto:|#)' || true)
done < <(git ls-files '*.md' | grep -vE '^(docs/superpowers/|\.agents/|.*/node_modules/)')
exit "$bad"
