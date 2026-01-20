@echo off
title Track Report changes with local git

echo Saving changes to tracked files

cd "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\"
git commit -am "Update Report on %date% %time%"
set success=%errorlevel%

echo Success code: %success%

if not %success%==0 goto :pauseExecution

echo Execution was successful!
goto :end

:pauseExecution
echo.
echo Something went wrong. Pausing execution.
pause

:end
timeout /t 5
cd %USERPROFILE%

echo.