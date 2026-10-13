#!/usr/bin/env bash
set -e

MANAGED_DIR="/Volumes/King's SSD/SteamLibrary/steamapps/common/Tavern Master/TavernMaster_Mac.app/Contents/Resources/Data/Managed"

echo "=== Tavern Master Mod Uninstaller ==="

if [ -f "$MANAGED_DIR/Assembly-CSharp.dll.bak" ]; then
    echo "Restoring original Assembly-CSharp.dll..."
    cp -f "$MANAGED_DIR/Assembly-CSharp.dll.bak" "$MANAGED_DIR/Assembly-CSharp.dll"
    rm -f "$MANAGED_DIR/TavernMod.dll"
    echo "Berhasil! Game telah dikembalikan ke kondisi asli (vanilla)."
else
    echo "File backup Assembly-CSharp.dll.bak tidak ditemukan."
fi
