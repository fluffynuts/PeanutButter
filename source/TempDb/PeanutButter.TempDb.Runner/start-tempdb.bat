@echo off

where /Q dotnet
if %ERRORLEVEL% NEQ 0 goto :no_dotnet

dotnet %~dp0\PeanutButter.TempDb.Runner.dll %*

goto end

:no_dotnet
echo dotnet must be found in your path to use this utility
exit /b 127

:end
