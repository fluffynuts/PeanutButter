@echo off

if exist PeanutButter.TempDb.Runner.exe (
    @rem running from net framework output
    PeanutButter.TempDb.Runner.exe %*
) else (
    @rem running from netcore output
    where /Q dotnet
    if %ERRORLEVEL% NEQ 0 goto :no_dotnet

    dotnet %~dp0\PeanutButter.TempDb.Runner.dll %*

    goto end

    :no_dotnet
    echo dotnet must be found in your path to use this utility
    exit /b 127

    :end
)
