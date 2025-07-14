#!/bin/bash
# "command not found" exit code, according to http://www.tldp.org/LDP/abs/html/exitcodes.html
ENOENT=127
# running from netcore output
if test -z "$(which dotnet)"; then
    echo "dotnet core is required in your path to use tempdb";
    # "command not found" exit code, according to http://www.tldp.org/LDP/abs/html/exitcodes.html
    exit ${ENOENT};
fi
dotnet "$(dirname $0)/PeanutButter.TempDb.Runner.dll" "$@"
