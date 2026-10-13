@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo     Tavern Master Mod Uninstaller (Windows)
echo ===================================================

set "MANAGED_DIR="

if exist "Tavern Master_Data\Managed" (
    set "MANAGED_DIR=%CD%\Tavern Master_Data\Managed"
) else if exist "..\Tavern Master_Data\Managed" (
    set "MANAGED_DIR=%CD%\..\Tavern Master_Data\Managed"
) else if exist "C:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed" (
    set "MANAGED_DIR=C:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
)

if "!MANAGED_DIR!"=="" (
    echo.
    echo Masukkan path folder Tavern Master_Data\Managed game Anda:
    set /p "MANAGED_DIR=Path folder: "
)

if exist "!MANAGED_DIR!\Assembly-CSharp.dll.bak" (
    echo.
    echo Mengembalikan file original vanilla...
    copy /Y "!MANAGED_DIR!\Assembly-CSharp.dll.bak" "!MANAGED_DIR!\Assembly-CSharp.dll"
    if exist "!MANAGED_DIR!\TavernMod.dll" del /F /Q "!MANAGED_DIR!\TavernMod.dll"
    echo Game berhasil dikembalikan ke kondisi asli!
) else (
    echo [ERROR] Backup Assembly-CSharp.dll.bak tidak ditemukan di: "!MANAGED_DIR!"
)

pause
