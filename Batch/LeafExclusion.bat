@echo off
title Leaf Exclusion

echo Retrieve Exclusions and save to report
"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" leafExclusion -f "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\LeafThreads.json" -o "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\ExclusionList.csv"
set exclusion=%errorlevel%

rem Return codes
echo Exclusion returned the following: %exclusion%

Rem Error handling
if not "%exclusion%"=="0" goto :pauseExecution

echo Executions successful!
goto :end

:pauseExecution
echo An execution failed
pause

:end
timeout /t 5
echo.