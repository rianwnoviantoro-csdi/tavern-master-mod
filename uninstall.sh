#!/usr/bin/env bash
set -e

echo "============================================================"
echo "        🏰 TAVERN MASTER - MOD UNINSTALLER (macOS)"
echo "============================================================"

MANAGED_DIR=""

if [ -n "$1" ]; then
    INPUT_PATH="$1"
    if [ -f "$INPUT_PATH/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$INPUT_PATH/Contents/Resources/Data/Managed"
    elif [ -f "$INPUT_PATH/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$INPUT_PATH"
    fi
fi

if [ -z "$MANAGED_DIR" ]; then
    for candidate in /Volumes/*/SteamLibrary/steamapps/common/"Tavern Master"/"TavernMaster_Mac.app"; do
        if [ -f "$candidate/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
            MANAGED_DIR="$candidate/Contents/Resources/Data/Managed"
            break
        fi
    done
fi

if [ -z "$MANAGED_DIR" ]; then
    DEFAULT_STEAM="$HOME/Library/Application Support/Steam/steamapps/common/Tavern Master/TavernMaster_Mac.app"
    if [ -f "$DEFAULT_STEAM/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$DEFAULT_STEAM/Contents/Resources/Data/Managed"
    fi
fi

if [ -z "$MANAGED_DIR" ]; then
    echo ""
    echo "Silakan seret (drag-and-drop) file 'TavernMaster_Mac.app' ke sini:"
    read -r USER_INPUT
    USER_INPUT="${USER_INPUT//\'/}"
    USER_INPUT="${USER_INPUT//\"/}"
    if [ -f "$USER_INPUT/Contents/Resources/Data/Managed/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$USER_INPUT/Contents/Resources/Data/Managed"
    elif [ -f "$USER_INPUT/Assembly-CSharp.dll" ]; then
        MANAGED_DIR="$USER_INPUT"
    fi
fi

if [ -f "$MANAGED_DIR/Assembly-CSharp.dll.bak" ]; then
    echo "Mengembalikan original Assembly-CSharp.dll..."
    cp -f "$MANAGED_DIR/Assembly-CSharp.dll.bak" "$MANAGED_DIR/Assembly-CSharp.dll"
    rm -f "$MANAGED_DIR/TavernMod.dll"
    echo ""
    echo "============================================================"
    echo "  BERHASIL! Game telah dikembalikan ke kondisi asli."
    echo "============================================================"
else
    echo "[ERROR] Backup Assembly-CSharp.dll.bak tidak ditemukan di: $MANAGED_DIR"
fi
