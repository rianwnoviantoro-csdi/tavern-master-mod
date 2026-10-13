@echo off
setlocal enabledelayedexpansion
title Tavern Master Mod Installer v1.0

echo ============================================================
echo         🏰 TAVERN MASTER - MOD INSTALLER v1.0
echo ============================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "MANAGED_DIR="

:: 1. Cek parameter jika user men-drag and drop folder/exe ke install.bat
if not "%~1"=="" (
    set "INPUT_PATH=%~1"
    if exist "!INPUT_PATH!\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!INPUT_PATH!\Tavern Master_Data\Managed"
    ) else if exist "!INPUT_PATH!\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!INPUT_PATH!\Managed"
    ) else if exist "!INPUT_PATH!\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!INPUT_PATH!"
    )
)

:: 2. Cek apakah folder installer ditaruh di dalam folder game Tavern Master
if "!MANAGED_DIR!"=="" (
    if exist "Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=%CD%\Tavern Master_Data\Managed"
    ) else if exist "..\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=%CD%\..\Tavern Master_Data\Managed"
    )
)

:: 3. Otomatis scan semua drive (C s/d Z) untuk mencari folder game Steam
if "!MANAGED_DIR!"=="" (
    echo [*] Sedang mendeteksi lokasi game Tavern Master di drive komputer Anda...
    for %%D in (C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        if exist "%%D:\" (
            if exist "%%D:\SteamLibrary\steamapps\common\Tavern Master\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
                set "MANAGED_DIR=%%D:\SteamLibrary\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
                goto :found
            )
            if exist "%%D:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
                set "MANAGED_DIR=%%D:\Program Files (x86)\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
                goto :found
            )
            if exist "%%D:\Program Files\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
                set "MANAGED_DIR=%%D:\Program Files\Steam\steamapps\common\Tavern Master\Tavern Master_Data\Managed"
                goto :found
            )
            if exist "%%D:\Games\Tavern Master\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
                set "MANAGED_DIR=%%D:\Games\Tavern Master\Tavern Master_Data\Managed"
                goto :found
            )
        )
    )
)

:found
:: 4. Jika tetap belum ditemukan otomatis, minta user memasukkan path
if "!MANAGED_DIR!"=="" (
    echo.
    echo [!] Folder game tidak ditemukan secara otomatis di drive standar.
    echo Silakan buka Steam ^> klik kanan Tavern Master ^> Manage ^> Browse local files,
    echo lalu seret (drag-and-drop) folder tersebut ke jendela ini:
    echo.
    set /p "USER_INPUT=Path folder game: "
    set "USER_INPUT=!USER_INPUT:"=!"
    
    if exist "!USER_INPUT!\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!\Tavern Master_Data\Managed"
    ) else if exist "!USER_INPUT!\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!\Managed"
    ) else if exist "!USER_INPUT!\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!"
    ) else (
        echo.
        echo [ERROR] Tidak dapat menemukan Assembly-CSharp.dll di lokasi tersebut.
        echo Pastikan path yang dimasukkan adalah folder Tavern Master.
        pause
        exit /b 1
    )
)

echo [V] Folder game terdeteksi:
echo     "!MANAGED_DIR!"
echo.

:: 5. Buat Backup file asli jika belum ada
echo [1/2] Memeriksa backup file asli...
if not exist "!MANAGED_DIR!\Assembly-CSharp.dll.bak" (
    copy /Y "!MANAGED_DIR!\Assembly-CSharp.dll" "!MANAGED_DIR!\Assembly-CSharp.dll.bak" >nul
    echo       -^> Backup berhasil dibuat: Assembly-CSharp.dll.bak
) else (
    echo       -^> Backup sudah ada sebelumnya.
)

:: 6. Salin file mod
echo.
echo [2/2] Memasang file mod...
copy /Y "%SCRIPT_DIR%Assembly-CSharp.dll" "!MANAGED_DIR!\Assembly-CSharp.dll" >nul
copy /Y "%SCRIPT_DIR%TavernMod.dll" "!MANAGED_DIR!\TavernMod.dll" >nul
echo       -^> TavernMod.dll dan Assembly-CSharp.dll berhasil disalin!

echo.
echo ============================================================
echo   BERHASIL! Mod Tavern Master v1.0 telah aktif.
echo   Buka game, tekan F1 / ~ atau klik [TAVERN MOD] di layar!
echo ============================================================
echo.
pause
