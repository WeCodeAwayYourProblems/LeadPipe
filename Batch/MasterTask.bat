@echo off
title Update repos and generate reports

call ".\Queries.bat"
set queries=%errorlevel%
call ".\ReleaseBuild.bat"
set build=%errorlevel%
call ".\ForceUpdateRepos.bat"
set repos=%errorlevel%
call ".\ReportGeneration.bat"
set reportGen=%errorlevel%
call ".\OpenFiles.bat"
set excelOpen=%errorlevel%
call ".\LeafExclusion.bat"
set exclusion=%errorlevel%
call ".\TrackReportChanges.bat"
set tracking=%errorlevel%

echo Were there errors in the build?
echo %build%

echo Were there errors in the queries?
echo %queries%

echo Were there errors in the repo updates?
echo %repos%

echo Were there errors in the report generation?
echo %reportGen%

echo Were there errors in the Excel opening and saving?
echo %excelOpen%

echo Were there errors in the Exclusion execution?
echo %exclusion%

echo Were there errors in tracking report changes?
echo %tracking%

echo.
pause