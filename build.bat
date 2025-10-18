@echo off
echo Building Proxy Collector...
dotnet build --configuration Release
if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo.
    echo To run the application:
    echo dotnet run --configuration Release
) else (
    echo Build failed!
    pause
)
