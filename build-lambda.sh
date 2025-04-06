#!/bin/bash

# Set variables
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PUBLISH_DIR="TogglTimesheet.Api/bin/Release/net8.0/publish"
OUTPUT_ZIP="$PUBLISH_DIR/package.zip"
TERRAFORM_DIR="$SCRIPT_DIR/terraform"
ASPNETCORE_ENVIRONMENT=${2:-Development}

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf $PUBLISH_DIR
rm -f $OUTPUT_ZIP

# Build and publish
echo "Building and publishing the project..."
dotnet publish -c Release TogglTimesheet.Api/TogglTimesheet.Api.csproj \
    -p:EnvironmentName=$ASPNETCORE_ENVIRONMENT

# Create zip package
echo "Creating Lambda package..."

# Change back to script directory and wait
cd "$SCRIPT_DIR"
echo "Waiting for files to be ready..."
sleep 2

# Create zip from original location
if [[ "$OSTYPE" == "msys"* ]] || [[ "$OSTYPE" == "cygwin"* ]]; then
    # Use PowerShell 7
    pwsh -Command "cd '$PUBLISH_DIR'; Compress-Archive -Path * -DestinationPath package.zip -Force -CompressionLevel Optimal"
else
    # Use zip on Unix-like systems
    (cd "$PUBLISH_DIR" && zip -r package.zip ./*)
fi

echo "Lambda package created at: $OUTPUT_ZIP"

# Check for deploy parameter
if [ "$1" = "-deploy" ]; then
    echo "Proceeding with Terraform initialization and apply..."
    cd "$TERRAFORM_DIR"
    terraform init
    terraform apply
else
    echo "Skip deploying. Use -deploy parameter to deploy to AWS."
fi
