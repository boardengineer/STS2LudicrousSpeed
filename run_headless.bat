@echo off
setlocal

set "GAME_EXE=C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe"
set "STS2_EXIT_WHEN_DONE=0"

if "%1"=="--exit-when-done" set "STS2_EXIT_WHEN_DONE=1"

cd /d "%~dp0"
dotnet build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

"%GAME_EXE%" --headless
