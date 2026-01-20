@echo off
title Update Release build
echo Ensure that the release build is up-to-date with debug

:: Move into the correct folder location
cd %USERPROFILE%\Repos\Automate

:: Move into the main branch of the repo
FOR /F "delims=" %%i IN ('git rev-parse --abbrev-ref HEAD') DO SET CURRENT_BRANCH=%%i
IF "%CURRENT_BRANCH%"=="main" (
    ECHO Current branch is main
) ELSE (
    ECHO Current branch is not main. It is %CURRENT_BRANCH%. Switching to main.
    git checkout main
)

:: Build release from main branch
dotnet build --configuration Release
set built=%errorlevel%

echo Were there execution errors?
echo %built%

if not "%built%"=="0" goto :pauseExecution

echo Successfully built Release!
goto :end

:pauseExecution
echo Build failure
pause

:gitfailure
echo Git failed to checkout into main branch. Please check on that
pause

:end
:: Move into the dev branch of the repo
FOR /F "delims=" %%i IN ('git rev-parse --abbrev-ref HEAD') DO SET CURRENT_BRANCH=%%i
IF "%CURRENT_BRANCH%"=="dev" (
    ECHO Current branch is dev
) ELSE (
    ECHO Current branch is not dev. It is %CURRENT_BRANCH%. Switching to dev.
    git checkout dev
)
timeout /t 5
echo.