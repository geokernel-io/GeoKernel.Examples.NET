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

set "PROJECT_COUNT=0"
for /r %%P in (*.csproj) do set /a PROJECT_COUNT+=1

if "%PROJECT_COUNT%"=="0" (
    echo ERROR: No project files were found under %CD%.
    exit /b 1
)

echo Found %PROJECT_COUNT% project files.
echo.

echo [2/3] Restoring all projects with forced package evaluation...
set "CURRENT_PROJECT=0"
for /r %%P in (*.csproj) do (
    set /a CURRENT_PROJECT+=1
    echo [Restore !CURRENT_PROJECT!/%PROJECT_COUNT%] %%~fP
    dotnet restore "%%~fP" --force-evaluate --nologo --verbosity minimal
    if errorlevel 1 (
        echo ERROR: Restore failed for "%%~fP"
        exit /b 1
    )
)
echo [2/3] Completed.
echo.

echo [3/3] Building all projects...
set "CURRENT_PROJECT=0"
for /r %%P in (*.csproj) do (
    set /a CURRENT_PROJECT+=1
    echo [Build !CURRENT_PROJECT!/%PROJECT_COUNT%] %%~fP
    dotnet build "%%~fP" --no-restore --nologo --verbosity minimal
    if errorlevel 1 (
        echo ERROR: Build failed for "%%~fP"
        exit /b 1
    )
)

echo.
echo ============================================================
echo SUCCESS: %PROJECT_COUNT% projects restored and built.
echo ============================================================
exit /b 0
