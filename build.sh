#!/usr/bin/env bash
set -euo pipefail

# Build script for SCON mod (Linux/macOS)
# Usage:
#   ./build.sh            # restore + build
#   ./build.sh --install  # build + copy dll into BepInEx/plugins (requires STATIONEERS_PATH)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

INSTALL=false
if [[ "${1:-}" == "--install" || "${1:-}" == "-i" ]]; then
  INSTALL=true
fi

echo "[SCON] Restoring packages..."
dotnet restore SCON.csproj

echo "[SCON] Building (Release)..."
dotnet build SCON.csproj -c Release

OUT_DLL="bin/Release/net472/SCON.dll"
if [[ ! -f "$OUT_DLL" ]]; then
  echo "[SCON] Build output not found: $OUT_DLL" >&2
  exit 1
fi

echo "[SCON] Build successful: $OUT_DLL"

if $INSTALL; then
  if [[ -z "${STATIONEERS_PATH:-}" ]]; then
    echo "[SCON] STATIONEERS_PATH is not set. Skipping install." >&2
    echo "Set it, for example:" >&2
    echo "  export STATIONEERS_PATH=\"$HOME/.local/share/Steam/steamapps/common/Stationeers\"" >&2
    exit 1
  fi

  PLUGINS_DIR="$STATIONEERS_PATH/BepInEx/plugins"
  if [[ ! -d "$PLUGINS_DIR" ]]; then
    echo "[SCON] BepInEx plugins folder not found at: $PLUGINS_DIR" >&2
    echo "Make sure BepInEx 5.x is installed in Stationeers." >&2
    exit 1
  fi

  echo "[SCON] Installing to $PLUGINS_DIR"
  cp -f "$OUT_DLL" "$PLUGINS_DIR/"
  echo "[SCON] Installation complete."
fi
