#!/usr/bin/env bash
# Fails when a relative link in a markdown file is broken, or a page in docs/ is linked from nowhere.
set -euo pipefail
cd "$(dirname "$0")/.."

bad=0
linked=$(mktemp)
trap 'rm -f "$linked"' EXIT

while IFS= read -r file; do
  dir=$(dirname "$file")
  while IFS= read -r link; do
    target=${link%%#*}
    [[ -n $target ]] || continue
    [[ -e $dir/$target ]] || {
      echo "$file: broken link $link"
      bad=1
      continue
    }
    realpath -m --relative-to=. "$dir/$target" >> "$linked"
  done < <(grep -oE '\]\([^)[:space:]]+\)' "$file" | sed -E 's/^\]\(//; s/\)$//' |
    grep -vE '^(https?:|mailto:|#)' || true)
done < <(git ls-files '*.md' | grep -vE '^(docs/superpowers/|\.agents/|.*/node_modules/)')

while IFS= read -r page; do
  grep -qxF "$page" "$linked" || {
    echo "$page: no page links to it"
    bad=1
  }
done < <(git ls-files 'docs/*.md' | grep -vE '^docs/(README\.md|superpowers/)')
exit "$bad"
