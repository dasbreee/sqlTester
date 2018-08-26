::This script is used to run the delta scripts locally after the dbs have been restored
::Copy this script next to the delta sql files and rename the DATABASENAME variable
::Thanks to Bob O for writing this script

set SERVERNAME=DESKTOP\MAIN
set DATABASENAME=MemeWallStreet
sqlcmd.exe -S %SERVERNAME% -d %DATABASENAME% -i .\Cadence.sql > %DATABASENAME%.log