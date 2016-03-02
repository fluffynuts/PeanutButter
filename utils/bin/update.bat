@echo off
set SRC=..\..\source\NugetPackageVersionIncrementer\bin\Debug
pushd %~dp0
copy %SRC%\*.exe .
copy %SRC%\*.dll .
copy %SRC%\*.config .
popd
@echo on
