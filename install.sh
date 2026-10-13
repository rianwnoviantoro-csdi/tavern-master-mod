#!/usr/bin/env bash
set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"

echo "============================================================"
echo "        🏰 TAVERN MASTER - MOD INSTALLER (macOS)"
echo "============================================================"

MANAGED_DIR=""

# 1. Parameter jika diberikan via CLI: ./install.sh "/path/to/game"
if [ -n "$1" ]; then
    INPUT_PATH="$1"
    if [ -f "$INPUT_PATH/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$INPUT_PATH/Contents/Resources/Data/Managed"
    elif [ -f "$INPUT_PATH/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$INPUT_PATH"
    fi
fi

# 2. Cek semua volume yang terpasang di Mac (/Volumes/*/SteamLibrary/...)
if [ -z "$MANAGED_DIR" ]; then
    for candidate in /Volumes/*/SteamLibrary/steamapps/common/"Tavern Master"/"TavernMaster_Mac.app"; do
        if [ -f "$candidate/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
            MANAGED_DIR="$candidate/Contents/Resources/Data/Managed"
            break
        fi
    done
fi

# 3. Cek lokasi standar macOS Steam di Library
if [ -z "$MANAGED_DIR" ]; then
    DEFAULT_STEAM="$HOME/Library/Application Support/Steam/steamapps/common/Tavern Master/TavernMaster_Mac.app"
    if [ -f "$DEFAULT_STEAM/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$DEFAULT_STEAM/Contents/Resources/Data/Managed"
    fi
fi

# 4. Cek folder /Applications
if [ -z "$MANAGED_DIR" ]; then
    if [ -f "/Applications/TavernMaster_Mac.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="/Applications/TavernMaster_Mac.app/Contents/Resources/Data/Managed"
    fi
fi

# 5. Jika belum ditemukan, minta input user (bisa drag & drop file .app ke Terminal)
if [ -z "$MANAGED_DIR" ]; then
    echo ""
    echo "[!] Game tidak ditemukan otomatis di lokasi standar."
    echo "Silakan seret (drag-and-drop) file 'TavernMaster_Mac.app' ke jendela Terminal ini:"
    read -r USER_INPUT
    USER_INPUT="${USER_INPUT//\'/}"
    USER_INPUT="${USER_INPUT//\"/}"
    if [ -f "$USER_INPUT/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$USER_INPUT/Contents/Resources/Data/Managed"
    elif [ -f "$USER_INPUT/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$USER_INPUT"
    else
        echo "[ERROR] Assembly-CSharp.dll tidak ditemukan di path tersebut!"
        exit 1
    fi
fi

echo "[V] Target game ditemukan di:"
echo "    $MANAGED_DIR"
echo ""

echo "[*] Mengompilasi mod..."
dotnet build "$DIR/TavernMod/TavernMod.csproj"

echo "[*] Menjalankan patcher..."
dotnet run --project "$DIR/Patcher/Patcher.csproj" -- "$MANAGED_DIR"

echo ""
echo "============================================================"
echo "  BERHASIL! Mod Tavern Master v1.0 telah aktif di macOS."
echo "============================================================"
