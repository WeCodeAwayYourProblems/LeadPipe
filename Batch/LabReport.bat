@echo off

title Lab Query

set BASE=%USERPROFILE%\Repos\LeadPipe\LeadPipe.Infrastructure\.info
set BASE2=%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports

set database="%BASE%\leadpipe.test.db"
set output="%BASE2%\LabReport_Test.csv"

sqlite3 -header -csv %database% < "%BASE%\LabReport.sql" > %output%

set error=%errorlevel%
echo.
echo Lab Query success: %error%
:: error messages are placed in the output file
if not "%error%"="0" (
    type %output%
    goto :pauseExecution
)
echo.

:: Ending
echo All Executions successful!
goto :end

:pauseExecution
echo Query failed
pause 

echo.
:end