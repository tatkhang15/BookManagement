@echo off
chcp 65001 >nul
title BookManagement - Building...
echo Dang build solution...
dotnet build "%~dp0BookManagement.slnx" -c Debug --verbosity quiet --nologo
if %ERRORLEVEL% NEQ 0 (
    echo Build that bai!
    pause
    exit /b 1
)
echo Build thanh cong!
pause
