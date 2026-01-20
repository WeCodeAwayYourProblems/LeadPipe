@echo off
title Execute daily queries

rem Inputs from Command Line
echo Please have your password ready
set /p host="Please enter the url of the database: "
set /p user="Please enter your username: "
set /p pass="Please enter your password: "

rem Active Not Termite
echo.
echo Active not Termite query
set notTermiteQuery="%USERPROFILE%\Repos\Sql-Queries\Current Customers by Service type\Active NOT Termite.sql"
set notTermiteOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\NotTermite.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %notTermiteQuery% > %notTermiteOutput%
set notTermiteErr=%errorlevel%
echo.
echo Not Termite Query success: %notTermiteErr%
echo Not Termite output: %notTermiteOutput%
rem error messages are placed in the output file
if not "%notTermiteErr%"=="0" type %notTermiteOutput% 
if not "%notTermiteErr%"=="0" goto :pauseExecution
echo.

rem Active Termite
echo.
echo Active Termite Query
set termiteQuery="%USERPROFILE%\Repos\Sql-Queries\Current Customers by Service type\Active Termite only.sql"
set termiteOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\Termite.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %termiteQuery% > %termiteOutput%
set termiteErr=%errorlevel%
echo.
echo Termite Query success: %termiteErr%
echo Termite output: %termiteOutput%
rem error messages are placed in the output file
if not "%termiteErr%"=="0" type %termiteOutput% 
if not "%termiteErr%"=="0" goto :pauseExecution
echo.

rem CornFormation
echo.
echo Corn Formation Query
set cornQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\CornFormation.sql"
set cornOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\CornFormationReport.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb --batch < %cornQuery% > %cornOutput%
set cornErr=%errorlevel%
echo.
echo Corn Query success: %cornErr%
echo Corn output: %cornOutput%
rem error messages are placed in the output file
if not "%cornErr%"=="0" type %cornOutput% 
if not "%cornErr%"=="0" goto :pauseExecution
echo.

rem GoonDoggle
echo.
echo GoonDoggle Query
set goonQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\GoonDoggle.sql"
set goonOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\GoonDoggleReport.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb --batch < %goonQuery% > %goonOutput%
set goonErr=%errorlevel%
echo.
echo Goon Query success: %goonErr%
echo Goon output: %goonOutput%
rem error messages are placed in the output file
if not "%goonErr%"=="0" type %goonOutput% 
if not "%goonErr%"=="0" goto :pauseExecution
echo.

rem MacBang
echo.
echo MacBang Query
set macBangQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\MacBang.sql"
set macBangOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\MacBangReport.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb --batch < %macBangQuery% > %macBangOutput%
set macBangErr=%errorlevel%
echo.
echo MacBang Query success: %macBangErr%
echo MacBang output: %macBangOutput%
rem error messages are placed in the output file
if not "%macBangErr%"=="0" type %macBangOutput% 
if not "%macBangErr%"=="0" goto :pauseExecution
echo.

rem PanFries
echo.
echo Pan Fries Query
set panQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\PanFries.sql"
set panOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\PanFriesReport.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb --batch < %panQuery% > %panOutput%
set panErr=%errorlevel%
echo.
echo Pan Query success: %panErr%
echo Pan output: %panOutput%
rem error messages are placed in the output file
if not "%panErr%"=="0" type %panOutput% 
if not "%panErr%"=="0" goto :pauseExecution
echo.

rem Lotus
echo.
echo Lotus Query
set lotusQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\Lotus.sql"
set lotusOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\LotusReport.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_internetmarketingdb --batch < %lotusQuery% > %lotusOutput%
set lotusErr=%errorlevel%
echo.
echo Lotus Query success: %lotusErr%
echo Lotus output: %lotusOutput%
rem error messages are placed in the output file
if not "%lotusErr%"=="0" type %lotusOutput% 
if not "%lotusErr%"=="0" goto :pauseExecution
echo.

rem KatharticSummary
echo.
echo KatharticSummary Query
set katharticQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\KatharticSummary.sql"
set katharticOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\KatharticSummary.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_ctmdb --batch < %katharticQuery% > %katharticOutput%
set katharticErr=%errorlevel%
echo.
echo KatharticSummary Query success: %katharticErr%
echo KatharticSummary output: %katharticOutput%
rem error messages are placed in the output file
if not "%katharticErr%"=="0" type %katharticOutput% 
if not "%katharticErr%"=="0" goto :pauseExecution
echo.

rem Upsilon
echo.
echo Upsilon Query
set upsilonQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\Upsilon.sql"
set upsilonOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\UpsilonOut.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_ctmdb --batch < %upsilonQuery% > %upsilonOutput%
set upsilonErr=%errorlevel%
echo.
echo Upsilon Query success: %upsilonErr%
echo Upsilon output: %upsilonOutput%
rem error messages are placed in the output file
if not "%upsilonErr%"=="0" type %upsilonOutput% 
if not "%upsilonErr%"=="0" goto :pauseExecution
echo.

rem Giggle
echo.
echo Giggle Custard
set custardQuery="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Queries\GiggleCustardQuery.sql"
set custardOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\GigglyCustard.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %custardQuery% > %custardOutput%
set custardErr=%errorlevel%
echo.
echo Custard Query success: %custardErr%
echo Custard output: %custardOutput%
rem error messages are placed in the output file
if not "%custardErr%"=="0" type %custardOutput% 
if not "%custardErr%"=="0" goto :pauseExecution
echo.

rem LeafQuery
echo.
echo LeafQuery
set leafQuery="%USERPROFILE%\Repos\Sql-Queries\LeafDataQuery.sql"
set leafOutput="%USERPROFILE%\Repos\Automate\Automate.Infrastructure\.info\Reports\QueryReports\LeafQueryOut.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %leafQuery% > %leafOutput%
set leafErr=%errorlevel%
echo.
echo Leaf Query success: %leafErr%
echo Leaf output: %leafOutput%
rem error messages are placed in the output file
if not "%leafErr%"=="0" type %leafOutput% 
if not "%leafErr%"=="0" goto :pauseExecution
echo.

rem Code1
echo.
echo Code HepYepNoTerms
set codeQuery="%USERPROFILE%\Repos\Sql-Queries\Code\HepYepNoTerms.sql"
set codeOutput="%USERPROFILE%\Repos\Sql-Queries\Code\HepYepNoTerms.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %codeQuery% > %codeOutput%
set codeErr=%errorlevel%
echo.
echo Code HepYepNoTerms Query success: %codeErr%
echo Code HepYepNoTerms output: %codeOutput%
rem error messages are placed in the output file
if not "%codeErr%"=="0" type %codeOutput% 
if not "%codeErr%"=="0" goto :pauseExecution
echo.

rem Code2
echo.
echo Code2 UnpurSubs
set code2Query="%USERPROFILE%\Repos\Sql-Queries\Code\UnPurSubs.sql"
set code2Output="%USERPROFILE%\Repos\Sql-Queries\Code\UnPurSubs.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %code2Query% > %code2Output%
set code2Err=%errorlevel%
echo.
echo Code2 UnpurSubs Query success: %code2Err%
echo Code2 UnpurSubs output: %code2Output%
rem error messages are placed in the output file
if not "%code2Err%"=="0" type %code2Output% 
if not "%code2Err%"=="0" goto :pauseExecution
echo.

rem Code3
echo.
echo Code3 Winner
set code3Query="%USERPROFILE%\Repos\Sql-Queries\Code\Winner.sql"
set code3Output="%USERPROFILE%\Repos\Sql-Queries\Code\Winner.tsv"
"C:\Program Files\MySQL\MySQl Workbench 8.0 CE\mysql.exe" -u %user% -p%pass% -h %host% -D dwh_reportsdb --batch < %code3Query% > %code3Output%
set code3Err=%errorlevel%
echo.
echo Code3 Winner Query success: %code3Err%
echo Code3 Winner output: %code3Output%
rem error messages are placed in the output file
if not "%code3Err%"=="0" type %code3Output% 
if not "%code3Err%"=="0" goto :pauseExecution
echo.

rem Ending
echo All Executions were successful!
goto :end

:pauseExecution
echo At least one execution failed
pause

:end
