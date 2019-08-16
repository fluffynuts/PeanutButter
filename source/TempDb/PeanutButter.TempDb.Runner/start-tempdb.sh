#!/bin/bash
if test -z "$(which dotnet)"; then
    echo "dotnet core is required in your path to use tempdb";
    # "command not found" exit code, according to http://www.tldp.org/LDP/abs/html/exitcodes.html
    exit 127;
fi
dotnet $(dirname $0)/PeanutButter.TempDb.Runner.dll $@