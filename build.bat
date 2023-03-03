@echo off
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false
dotnet publish -c Release -r win-x86 -p:PublishSingleFile=true --self-contained false
explorer .\FiveMArtifactsDownloader\bin\Release\net6.0\