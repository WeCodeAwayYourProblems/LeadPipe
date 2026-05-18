@echo off

set BASE=%USERPROFILE%\Repos\LeadPipe\LeadPipe.Infrastructure\.info
set BASE2=%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports

sqlite3 -header -csv "%BASE%\leadpipe.test.db" < "%BASE%\LabReport.sql" > "%BASE2%\LabReport_Test.csv"