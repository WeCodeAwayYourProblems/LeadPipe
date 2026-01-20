@echo off
title Manual Lotus Query

echo Please have your password ready
set /p host="Please enter the url of the database: "
set /p user="Please enter your username: "
set /p pass="Please enter your password: "

pause 

rem Lotus
echo Lotus Query
set lotusOut=%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\LotusReport.tsv
set lotusQuery=%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\Lotus.sql
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb < %lotusQuery% --batch > %lotusOut%
set lotus=%errorlevel%

echo Lotus success: %lotus%
echo Lotus output: %lotusOut%

pause