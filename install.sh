#!/usr/bin/env bash
set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"

echo "=== Building & Installing Tavern Master Mod ==="
dotnet build "$DIR/TavernMod/TavernMod.csproj"
dotnet run --project "$DIR/Patcher/Patcher.csproj"
echo "Selesai! Mod berhasil dipasang."
