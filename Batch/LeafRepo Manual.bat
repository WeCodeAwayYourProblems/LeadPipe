@echo off
call ".\LeafRepo.bat"
set success=%errorlevel%

echo Leaf repo success code:
echo %success%

if not "%success%"=="0" goto :notSuccess

echo Success!
goto :end

:notSuccess
echo Execution was not successful

:end
pause