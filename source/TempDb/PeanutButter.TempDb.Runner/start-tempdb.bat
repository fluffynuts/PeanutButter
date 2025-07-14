@echo off

@rem running from netcore output
where /Q dotnet
if ERRORLEVEL 0 (
   dotnet %~dp0\PeanutButter.TempDb.Runner.dll %*
) else (
   echo dotnet must be found in your path to use this utility
   exit /b 127
)
