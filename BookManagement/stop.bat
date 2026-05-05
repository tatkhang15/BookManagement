@echo off
chcp 65001 >nul
title BookManagement - Stopping...
echo ============================================
echo   BookManagement - Dừng ứng dụng
echo ============================================
echo.
echo Đang dừng tất cả dotnet processes...
taskkill /IM dotnet.exe /F >nul 2>&1
echo Đã dừng thành công!
echo.
pause
