@echo off
setlocal

rem Set variables
set PUBLISH_DIR=TogglTimesheet.Api\bin\Release\net8.0\publish
set OUTPUT_ZIP=%PUBLISH_DIR%\package.zip

rem Clean previous builds
echo Cleaning previous builds...
if exist %PUBLISH_DIR% rd /s /q %PUBLISH_DIR%
if exist %OUTPUT_ZIP% del /f /q %OUTPUT_ZIP%

rem Build and publish
echo Building and publishing the project...
dotnet publish -c Release TogglTimesheet.Api\TogglTimesheet.Api.csproj

rem Create zip package
echo Creating Lambda package...

rem Change back to original directory and wait
cd %~dp0
echo Waiting for files to be ready...
timeout /t 2 /nobreak >nul

pwsh -Command "cd '%PUBLISH_DIR%'; Compress-Archive -Path * -DestinationPath package.zip -Force -CompressionLevel Optimal"

echo Lambda package created at: %OUTPUT_ZIP%

IF "%1"=="-deploy" (
    echo Proceeding with Terraform initialization and apply...
    cd terraform
    terraform init
    terraform apply
) ELSE (
    echo Skip deploying. Use -deploy parameter to deploy to AWS.
)
