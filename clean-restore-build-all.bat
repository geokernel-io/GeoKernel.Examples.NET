@echo off
setlocal EnableExtensions EnableDelayedExpansion

cd /d "%~dp0"

echo ============================================================
echo GeoKernel.Examples.NET - Clean, Restore, Build All Projects
echo Root: %CD%
echo ============================================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet CLI was not found in PATH.
    exit /b 1
)

echo [1/3] Removing project bin and obj directories...
for /d /r %%D in (bin obj) do (
    if exist "%%~fD" (
        echo Removing "%%~fD"
        rmdir /s /q "%%~fD"
        if exist "%%~fD" (
            echo ERROR: Could not remove "%%~fD"
            exit /b 1
        )
    )
)
echo [1/3] Completed.
echo.

if /i "%~1"=="clean-only" (
    echo ============================================================
    echo SUCCESS: All bin and obj directories were removed.
    echo ============================================================
    exit /b 0
)
