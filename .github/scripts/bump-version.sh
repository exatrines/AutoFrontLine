#!/usr/bin/env bash
# Usage: bash .github/scripts/bump-version.sh 1.0.0.0
set -euo pipefail

MANIFEST="AutoFrontline/AutoFrontline.json"

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

CSPROJ_VERSION="${VERSION%.*}"
DOWNLOAD_URL="https://github.com/exatrines/AutoFrontline/releases/download/${TAG}/AutoFrontline.zip"

jq --arg av "$VERSION" --arg url "$DOWNLOAD_URL" \
  '.AssemblyVersion = $av
   | .DownloadLinkInstall = $url
   | .DownloadLinkUpdate = $url' \
  "$MANIFEST" > "${MANIFEST}.tmp"
mv "${MANIFEST}.tmp" "$MANIFEST"

py - <<PY
import pathlib
import re

path = pathlib.Path("AutoFrontline/AutoFrontline.csproj")
text = path.read_text(encoding="utf-8")
text, n = re.subn(r"(<Version>)[^<]+(</Version>)", rf"\g<1>${CSPROJ_VERSION}\2", text, count=1)
if n != 1:
    raise SystemExit("Could not update <Version> in csproj")
path.write_text(text, encoding="utf-8")
PY

echo "Updated to $VERSION (tag $TAG)"
echo "  $MANIFEST"
echo "  AutoFrontline/AutoFrontline.csproj (<Version>${CSPROJ_VERSION}</Version>)"
echo ""
echo "Update pluginmaster.json in your external repo separately."
echo ""
echo "Next:"
echo "  git add $MANIFEST AutoFrontline/AutoFrontline.csproj CHANGELOG.md"
echo "  git commit -m \"Release ${VERSION}\""
echo "  git tag ${TAG}"
echo "  git push origin main"
echo "  git push origin ${TAG}"
