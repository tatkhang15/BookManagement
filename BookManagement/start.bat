@echo off
chcp 65001 >nul
title BookManagement - Starting...
echo ============================================
echo   BookManagement - Khoi dong ung dung
echo ============================================
echo.

set API_DLL=%~dp0BookManagement.Api\bin\Debug\net8.0\BookManagement.Api.dll
set WEB_DLL=%~dp0BookManagement.Web\bin\Debug\net8.0\BookManagement.Web.dll

if not exist "%API_DLL%" goto :BUILD
if not exist "%WEB_DLL%" goto :BUILD
echo Da co san build, khoi dong ngay...
goto :RUN

:BUILD
echo Chua co build, dang build lan dau...
dotnet build "%~dp0BookManagement.slnx" -c Debug --verbosity quiet --nologo
if %ERRORLEVEL% NEQ 0 (
    echo Build that bai!
    pause
    exit /b 1
)
echo Build thanh cong!
echo.

:RUN
echo Dang khoi dong API va Web...
start /min "BookManagement.Api" "%~dp0BookManagement.Api\run-api.bat"
start /min "BookManagement.Web" "%~dp0BookManagement.Web\run-web.bat"

echo Cho server khoi dong...
ping 127.0.0.1 -n 4 >nul
start http://localhost:5254

echo ============================================
echo   Da khoi dong thanh cong!
echo   De tat: chay stop.bat
echo   API: http://localhost:5104
echo   Web: http://localhost:5254
echo ============================================
echo.
echo Nhan phim bat ky de dong cua so nay...
pause >nul
