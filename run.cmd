start "Frontend" /d Frontend dotnet Frontend.dll
start "Backend" /d Backend dotnet Backend.dll
start "TextListener" /d TextListener dotnet TextListener.dll
start "TextRankCalc" /d TextRankCalc dotnet TextRankCalc.dll
start "TextStatistics" /d TextStatistics dotnet TextStatistics.dll

set configFile=%CD%\config.txt
for /f "tokens=1,2 delims=:" %%a in (%configFile%) do (
for /l %%i in (1, 1, %%b) do start "%%a" /d %%a dotnet %%a.dll
)