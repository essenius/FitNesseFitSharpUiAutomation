@echo off
setlocal
echo.%*
set /a number=%2 + 0
if /I [%1] EQU [error] exit %number%
if /I [%1] NEQ [wait] exit 0
if %number% EQU 0 exit 0
timeout %number%
