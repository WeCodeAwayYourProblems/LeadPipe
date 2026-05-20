@echo off
title All Report
echo Executing LeadPipe queries

set base=%USERPROFILE%\Repos
set outBase=%base%\Automate\Automate.Infrastructure\.info\Reports
set common=%base%\LeadPipe\LeadPipe.Infrastructure
set queries=%common%\.queries
set database=%common%\.info\leadpipe.test.db

:: --- Query 1 ---
set queryName=All Query
set output=%outBase%\AllReport_Test.csv
set sql=%queries%\AllReport.sql
call :runQuery
if errorlevel 1 goto :pauseExecution

:: --- Query 2 ---
set queryName=Yeller Corn Query
set output=%outBase%\YellerCorn_Test.csv
set sql=%queries%\YellerCorns.sql
call :runQuery
if errorlevel 1 goto :pauseExecution

echo All queries completed successfully!
goto :EOF

:runQuery
echo Running %queryName%
sqlite3 -header -csv %database% < %sql% > %output% 2>&1
if errorlevel 1 (
    echo.
    echo %queryName% failed!
    echo See the error message in %output%
    exit /b 1
)
echo %queryName% execution successful!
echo Result output to %output%
exit /b 0

:pauseExecution
echo.
pause
goto :EOF