@echo off
setlocal enabledelayedexpansion
title Tavern Master Mod Uninstaller

echo ============================================================
echo        🏰 TAVERN MASTER - MOD UNINSTALLER
echo ============================================================
echo.

set "MANAGED_DIR="

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

if "!MANAGED_DIR!"=="" (
    if exist "Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=%CD%\Tavern Master_Data\Managed"
    ) else if exist "..\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=%CD%\..\Tavern Master_Data\Managed"
    )
)

if "!MANAGED_DIR!"=="" (
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
if "!MANAGED_DIR!"=="" (
    echo.
    echo Masukkan path folder Tavern Master game Anda (bisa drag-and-drop):
    set /p "USER_INPUT=Path folder: "
    set "USER_INPUT=!USER_INPUT:"=!"
    if exist "!USER_INPUT!\Tavern Master_Data\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!\Tavern Master_Data\Managed"
    ) else if exist "!USER_INPUT!\Managed\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!\Managed"
    ) else if exist "!USER_INPUT!\Assembly-CSharp.dll" (
        set "MANAGED_DIR=!USER_INPUT!"
    )
)

if exist "!MANAGED_DIR!\Assembly-CSharp.dll.bak" (
    echo.
    echo Mengembalikan file original vanilla...
    copy /Y "!MANAGED_DIR!\Assembly-CSharp.dll.bak" "!MANAGED_DIR!\Assembly-CSharp.dll" >nul
    if exist "!MANAGED_DIR!\TavernMod.dll" del /F /Q "!MANAGED_DIR!\TavernMod.dll" >nul
    echo.
    echo ============================================================
    echo   BERHASIL! Game telah dikembalikan ke kondisi asli (vanilla).
    echo ============================================================
) else (
    echo.
    echo [ERROR] Backup Assembly-CSharp.dll.bak tidak ditemukan di:
    echo "!MANAGED_DIR!"
)

echo.
pause
