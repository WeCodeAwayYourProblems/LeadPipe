@echo off
title Generate Reports 
echo Please be sure to refresh the following message lists: LeafRepo Truncated, Leased, Libacion, Pan
echo.
Pause

rem Leaf Report
rem "%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" analyzeMessages -c "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CallRepo.json" -q "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CustomerRepo.json" -s "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\LeafMessages.csv" -t LeafRepo -axo "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\TextReport_LF.csv" -O "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\LeafReport_Gads_Truncated180.csv"
rem set leafrepoReport=%errorlevel%
rem echo.

rem Leased Report
"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" analyzeMessages -c "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CallRepo.json" -q "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CustomerRepo.json" -s "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\MessageAnalysis\LeasedMessagesInput.csv" -t Leased -o "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\LeasedMessages.csv"
set leasedReport=%errorlevel%
echo.

rem Libacion Report
"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" analyzeMessages -c "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CallRepo.json" -q "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CustomerRepo.json" -s "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\MessageAnalysis\LibacionForm.csv " -t Libacion -o "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\LibacionForm.csv"
set libacionReport=%errorlevel%
echo.

rem Pan Report
"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" analyzeMessages -c "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CallRepo.json" -q "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CustomerRepo.json" -s "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\MessageAnalysis\PNContactForms.csv" -t Pan -o "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\PanReport.csv"
set panReport=%errorlevel%
echo.

rem CalliValley
"%USERPROFILE%\Repos\Automate\Automate.Cli\bin\Release\net8.0\Automate.Cli.exe" analyzeMessages -c "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CallRepo.json" -q "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\ApiRepos\CustomerRepo.json" -s "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\MessageAnalysis\CalliValleyInput.csv" -t CalliValley -o "%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\CalliValley.csv"
set calliValley=%errorlevel%
echo.

echo Return codes. Return code 0 indicates success. Any other code indicates failure.
rem echo leafReport generation returned %leafrepoReport%
echo leasedReport generation returned %leasedReport%
echo libacionReport returned %libacionReport%
echo panReport returned %panReport%
echo CalliValley returned %calliValley%

rem if not "%leafrepoReport%"=="0" goto :pauseExecution
if not "%leasedReport%"=="0" goto :pauseExecution
if not "%libacionReport%"=="0" goto :pauseExecution
if not "%panReport%"=="0" goto :pauseExecution
if not "%calliValley%"=="0" goto :pauseExecution

echo All Executions were successful!
goto :end

:pauseExecution
echo At least one execution failed
pause

:end
timeout /t 5
echo.
