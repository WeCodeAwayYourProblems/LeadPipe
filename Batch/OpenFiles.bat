@echo off
echo Opening Excel file...

:: Define the script path
set "SCRIPT_PATH=%USERPROFILE%\Repos\Automate\SaveAndCloseExcel.ps1"

:: Check if the PowerShell script exists
if exist "%SCRIPT_PATH%" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_PATH%"
    
    :: Check for errors
    if %ERRORLEVEL% NEQ 0 (
        echo PowerShell script encountered an error. Pausing for review...
        pause
    ) else (
        echo Script completed successfully.
    )
) else (
    echo PowerShell script not found!
)

echo Done.
timeout /t 5
echo. 
