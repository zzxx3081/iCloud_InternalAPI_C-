@echo off
dotnet new nunit -n PlaywrightTests
cd PlaywrightTests
dotnet add package Microsoft.Playwright --version 1.28.0
dotnet build
cd bin\Debug\net6.0
powershell.exe .\playwright.ps1 install --force