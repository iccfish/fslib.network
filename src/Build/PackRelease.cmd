@ECHO OFF
cd "%~dp0"
cd ..\FSLib.Network

dotnet pack -c Release
