@echo off
set ASPNETCORE_URLS=http://localhost:5254
set ASPNETCORE_ENVIRONMENT=Development
cd /d %~dp0
dotnet bin\Debug\net8.0\BookManagement.Web.dll
