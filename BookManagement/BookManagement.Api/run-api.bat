@echo off
set ASPNETCORE_URLS=http://localhost:5104
set ASPNETCORE_ENVIRONMENT=Development
cd /d %~dp0
dotnet bin\Debug\net8.0\BookManagement.Api.dll
