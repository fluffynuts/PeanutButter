#!/bin/bash
RUNNER_EXE=$(dirname $0)/PeanutButter.TempDb.Runner.exe
# "command not found" exit code, according to http://www.tldp.org/LDP/abs/html/exitcodes.html
ENOENT=127
if test -e $RUNNER_EXE; then
    # running from net framework output
    if test -z "$(which mono)"; then
        echo "mono is required in your path to use tempdb"
        exit #ENOENT;
    fi
    mono $RUNNER_EXE $@
else
    # running from netcore output
    if test -z "$(which dotnet)"; then
        echo "dotnet core is required in your path to use tempdb";
        # "command not found" exit code, according to http://www.tldp.org/LDP/abs/html/exitcodes.html
        exit $ENOENT;
    fi
    dotnet $(dirname $0)/PeanutButter.TempDb.Runner.dll $@
fi
