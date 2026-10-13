@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo     Tavern Master Mod Installer (Windows)
echo ===================================================

set "SCRIPT_DIR=%~dp0"
set "MANAGED_DIR="

:: Check common locations
if exist "Tavern Master_Data\Managed" (
    set "MANAGED_DIR=%CD%\Tavern Master_Data\Managed"
) else if exist "..\Tavern Master_Data\Managed" (
    set "MANAGED_DIR=%CD%\..\Tavern Master_Data\Managed"
) else if exist "..\steamapps\common\Tavern Master\Tavern Master_Data\Managed" (
    set "MANAGED_DIR=%CD%\..\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
) else if exist "C:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed" (
    set "MANAGED_DIR=C:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
)

if "!MANAGED_DIR!"=="" (
    echo.
    echo Masukkan path folder Tavern Master_Data\Managed game Anda.
    echo Contoh: D:\SteamLibrary\steamapps\common\Tavern Master\Tavern Master_Data\Managed
    set /p "MANAGED_DIR=Path folder: "
)

if not exist "!MANAGED_DIR!\Assembly-CSharp.dll" (
    echo [ERROR] Assembly-CSharp.dll tidak ditemukan di: "!MANAGED_DIR!"
    pause
    exit /b 1
)

echo.
echo [1/2] Membuat backup Assembly-CSharp.dll.bak jika belum ada...
if not exist "!MANAGED_DIR!\Assembly-CSharp.dll.bak" (
    copy /Y "!MANAGED_DIR!\Assembly-CSharp.dll" "!MANAGED_DIR!\Assembly-CSharp.dll.bak"
    echo Backup berhasil dibuat.
) else (
    echo Backup sudah ada sebelumnya.
)

echo.
echo [2/2] Memasang file mod ke game...
copy /Y "%SCRIPT_DIR%Assembly-CSharp.dll" "!MANAGED_DIR!\Assembly-CSharp.dll"
copy /Y "%SCRIPT_DIR%TavernMod.dll" "!MANAGED_DIR!\TavernMod.dll"

echo.
echo ===================================================
echo  BERHASIL! Mod telah terpasang di Windows.
echo  Buka game dan tekan F1 / ~ atau klik tombol MOD di layar.
echo ===================================================
pause
