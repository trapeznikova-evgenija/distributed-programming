set version=%~1
set currentPath=%CD%
set buildFolderDir=version_%version%

set backendFolderName=Backend
set frontendFolderName=Frontend
set textListenerFolderName=TextListener
set textRankCalcFolderName=TextRankCalc
set vowelConsCounterFolderName=VowelConsCounter
set vowelConsRaterFolderName=VowelConsRater


if %1 == "" goto write_version
if exist "%currentPath%\%backendFolderName%" goto build_exists
mkdir "%buildFolderDir%"

copy "run.cmd" %currentPath%\%buildFolderDir%
copy "config.txt" %currentPath%\%buildFolderDir%

call :create_stop_script
call :start_build

:write_version
echo write application version
exit

:build_exists
echo %version% folder already exist
exit                             

:create_stop_script
  (
    echo taskkill /im dotnet.exe
  ) > "%buildFolderDir%/stop.cmd"
exit /b 0

:start_build
dotnet build "src/%backendFolderName%" -o "../../%buildFolderDir%/%backendFolderName%"
dotnet build "src/%frontendFolderName%" -o "../../%buildFolderDir%/%frontendFolderName%"
dotnet build "src/%textListenerFolderName%" -o "../../%buildFolderDir%/%textListenerFolderName%"
dotnet build "src/%textRankCalcFolderName%" -o "../../%buildFolderDir%/%textRankCalcFolderName%"
dotnet build "src/%vowelConsCounterFolderName%" -o "../../%buildFolderDir%/%vowelConsCounterFolderName%"
dotnet build "src/%vowelConsRaterFolderName%" -o "../../%buildFolderDir%/%vowelConsRaterFolderName%"
exit /b 0

:run_file_not_exist
echo run file not exitst in directory "%currentPath%\run.cmd"
exit