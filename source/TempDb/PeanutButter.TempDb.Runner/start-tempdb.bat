@echo off

if exist "PeanutButter.TempDb.Runner.exe" (
    @rem running from net framework output
    PeanutButter.TempDb.Runner.exe %*
) else (
    @rem running from netcore output
    where /Q dotnet
    if ERRORLEVEL 0 (
       dotnet %~dp0\PeanutButter.TempDb.Runner.dll %*
    ) else (
       echo dotnet must be found in your path to use this utility
       exit /b 127
    )
)
