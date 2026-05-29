#!/usr/bin/env bash
set -euo pipefail

SOURCE_MANIFEST="AutoFrontline/AutoFrontline.json"
OUTPUT_MANIFEST="${1:-AutoFrontline/bin/Release/AutoFrontline.json}"

if [[ -n "${RELEASE_VERSION:-}" ]]; then
  ASSEMBLY_VERSION="$RELEASE_VERSION"
else
  ASSEMBLY_VERSION=$(jq -r '.AssemblyVersion' "$SOURCE_MANIFEST")
fi

DALAMUD_API=$(jq -r '.DalamudApiLevel' "$SOURCE_MANIFEST")

if [[ -z "$ASSEMBLY_VERSION" || "$ASSEMBLY_VERSION" == "null" ]]; then
  echo "AssemblyVersion not found in $SOURCE_MANIFEST" >&2
  exit 1
fi

if [[ -z "$DALAMUD_API" || "$DALAMUD_API" == "null" ]]; then
  echo "DalamudApiLevel not found in $SOURCE_MANIFEST" >&2
  exit 1
fi

jq --arg av "$ASSEMBLY_VERSION" --arg api "$DALAMUD_API" \
  '.AssemblyVersion = $av | .DalamudApiLevel = ($api | tonumber)' \
  "$OUTPUT_MANIFEST" > "${OUTPUT_MANIFEST}.tmp"
mv "${OUTPUT_MANIFEST}.tmp" "$OUTPUT_MANIFEST"

echo "Synced $OUTPUT_MANIFEST: AssemblyVersion=$ASSEMBLY_VERSION DalamudApiLevel=$DALAMUD_API"
