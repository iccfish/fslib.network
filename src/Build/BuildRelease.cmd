@ECHO OFF
cd "%~dp0"
cd ..\FSLib.Network

dotnet build -c Release
