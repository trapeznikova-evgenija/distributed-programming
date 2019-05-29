set version=%~1
set currentPath=%CD%
set buildFolderDir=version_%version%

set backendFolderName=Backend
set frontendFolderName=Frontend
set textListenerFolderName=TextListener

if %1 == "" goto write_version
if exist "%currentPath%\%backendFolderName%" goto build_exists
mkdir "%buildFolderDir%"
call :create_run_script
call :create_stop_script
call :start_build

:write_version
echo write application version
exit

:build_exists
echo %version% folder already exist
exit

:create_run_script
  (
    echo start /d %frontendFolderName% dotnet %frontendFolderName%.dll
    echo start /d %backendFolderName% dotnet %backendFolderName%.dll 
    echo start /d %textListenerFolderName% dotnet %textListenerFolderName%.dll 
  ) > "%buildFolderDir%/run.cmd"
exit /b 0

:create_stop_script
  (
    echo taskkill /im dotnet.exe
  ) > "%buildFolderDir%/stop.cmd"
exit /b 0

:start_build
dotnet build "src/%backendFolderName%" -o "../../%buildFolderDir%/%backendFolderName%"
dotnet build "src/%frontendFolderName%" -o "../../%buildFolderDir%/%frontendFolderName%"
dotnet build "src/%textListenerFolderName%" -o "../../%buildFolderDir%/%textListenerFolderName%"
exit /b 0