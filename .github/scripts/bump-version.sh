#!/usr/bin/env bash
# Usage: bash .github/scripts/bump-version.sh 1.0.0.0
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <AssemblyVersion>" >&2
  echo "Example: $0 1.0.0.0" >&2
  exit 1
fi

VERSION="$1"
TAG="v${VERSION}"

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Version must be four numbers, e.g. 1.0.0.0" >&2
  exit 1
fi

# csproj uses up to three segments (1.0.0 -> assembly 1.0.0.0)
CSPROJ_VERSION="${VERSION%.*}"

DOWNLOAD_URL="https://github.com/exatrines/AutoFrontLine/releases/download/${TAG}/AutoFrontLine.zip"

jq --arg av "$VERSION" --arg url "$DOWNLOAD_URL" \
  '.[0].AssemblyVersion = $av
   | .[0].DownloadLinkInstall = $url
   | .[0].DownloadLinkUpdate = $url' \
  pluginmaster.json > pluginmaster.json.tmp
mv pluginmaster.json.tmp pluginmaster.json

jq --arg av "$VERSION" '.AssemblyVersion = $av' \
  AutoFrontLine/AutoFrontLine.json > AutoFrontLine/AutoFrontLine.json.tmp
mv AutoFrontLine/AutoFrontLine.json.tmp AutoFrontLine/AutoFrontLine.json

py - <<PY
import pathlib
import re

path = pathlib.Path("AutoFrontLine/AutoFrontLine.csproj")
text = path.read_text(encoding="utf-8")
text, n = re.subn(r"(<Version>)[^<]+(</Version>)", rf"\g<1>${CSPROJ_VERSION}\2", text, count=1)
if n != 1:
    raise SystemExit("Could not update <Version> in csproj")
path.write_text(text, encoding="utf-8")
PY

echo "Updated to $VERSION (tag $TAG)"
echo "  pluginmaster.json"
echo "  AutoFrontLine/AutoFrontLine.json"
echo "  AutoFrontLine/AutoFrontLine.csproj (<Version>${CSPROJ_VERSION}</Version>)"
echo ""
echo "Next:"
echo "  git add pluginmaster.json AutoFrontLine/AutoFrontLine.json AutoFrontLine/AutoFrontLine.csproj"
echo "  git commit -m \"Release ${VERSION}\""
echo "  git tag ${TAG}"
echo "  git push origin main --follow-tags"
