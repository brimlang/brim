#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION_FILE="$ROOT/toolchain/binaryen.version"
VERSION="$(tr -d '\n' < "$VERSION_FILE")"

uname_s="$(uname -s)"
uname_m="$(uname -m)"
case "$uname_s" in
  Linux)
    OS=linux
    LIB_VAR=LD_LIBRARY_PATH
    ;;
  Darwin)
    OS=macos
    LIB_VAR=DYLD_LIBRARY_PATH
    ;;
  *)
    echo "Unsupported OS: $uname_s" >&2
    exit 1
    ;;
esac

case "$uname_m" in
  x86_64|amd64)
    ARCH=x86_64
    RID="$OS-x64"
    ;;
  arm64|aarch64)
    ARCH=arm64
    RID="$OS-arm64"
    ;;
  *)
    echo "Unsupported architecture: $uname_m" >&2
    exit 1
    ;;
esac

EXT="tar.gz"
FILE="binaryen-${VERSION}-${ARCH}-${OS}.${EXT}"
URL="https://github.com/WebAssembly/binaryen/releases/download/${VERSION}/${FILE}"
DEST="$ROOT/toolchain/binaryen/${RID}"

mkdir -p "$DEST"
TMPDIR="$(mktemp -d)"
ARCHIVE="$TMPDIR/$FILE"
if [ ! -d "$DEST/bin" ]; then
  echo "Downloading $URL" >&2
  curl -Ls "$URL" -o "$ARCHIVE"
  if curl -Ls "$URL.sha256" -o "$ARCHIVE.sha256"; then
    (cd "$TMPDIR" && sha256sum -c "$ARCHIVE.sha256")
  fi
  tar -xf "$ARCHIVE" -C "$DEST" --strip-components=1
fi
rm -rf "$TMPDIR"

export BINARYEN_HOME="$DEST"
export PATH="$DEST/bin:${PATH}"
export $LIB_VAR="$DEST/lib:${!LIB_VAR:-}"
