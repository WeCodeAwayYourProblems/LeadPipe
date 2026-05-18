@echo off
title Update Automate Release build
set repo="Automate"
cd %USERPROFILE%\Repos\Automate

:beginning
echo Ensure that the %repo% release build is up-to-date with debug

FOR /F "delims=" %%i IN ('git rev-parse --abbrev-ref HEAD') DO SET CURRENT_BRANCH=%%i
IF "%CURRENT_BRANCH%"=="main" (
    ECHO Current branch is main
) ELSE (
    ECHO Switching to main from %CURRENT_BRANCH%.
    git checkout main
    IF ERRORLEVEL 1 goto :gitfailure
)

dotnet build --configuration Release
IF ERRORLEVEL 1 goto :pauseExecution
echo Successfully built Release!

:: Switch back to dev before potentially moving on
FOR /F "delims=" %%i IN ('git rev-parse --abbrev-ref HEAD') DO SET CURRENT_BRANCH=%%i
IF "%CURRENT_BRANCH%"=="dev" (
    ECHO Current branch is dev
) ELSE (
    ECHO Switching back to dev.
    git checkout dev
    IF ERRORLEVEL 1 goto :gitfailure
)

echo.
echo.

IF %repo%=="LeadPipe" (
    ECHO Finished LeadPipe build. Ending build process.
    goto :end
) ELSE (
    ECHO Finished %repo% build.
)

:: For extensibility, various local builds with main and dev branches will go here
title Update LeadPipe Release Build
set repo="LeadPipe"
echo Moving to %repo% build
cd %USERPROFILE%\Repos\LeadPipe
goto :beginning

:pauseExecution
echo Build failure in %repo%
pause
goto :end

:gitfailure
echo Git failed to switch branches in %repo%. Please check on that.
pause
goto :end

:end
echo.
echo Done.