#!/bin/bash

# Exit on any error
set -e

# Check if Toggl credentials are provided
if [ -z "$TOGGL_API_TOKEN" ] || [ -z "$TOGGL_WORKSPACE_ID" ]; then
    echo "❌ Error: TOGGL_API_TOKEN and TOGGL_WORKSPACE_ID environment variables must be set"
    echo "Example usage:"
    echo "TOGGL_API_TOKEN=your_token TOGGL_WORKSPACE_ID=your_workspace_id [TOGGL_TASK_RULE_FILE=path/to/rules.json] ./deploy.sh"
    exit 1
fi

# Export Terraform variables
export TF_VAR_toggl_api_token="$TOGGL_API_TOKEN"
export TF_VAR_toggl_workspace_id="$TOGGL_WORKSPACE_ID"

# Export task rule file path if provided
if [ ! -z "$TOGGL_TASK_RULE_FILE" ]; then
    export TF_VAR_toggl_task_rule_file="$TOGGL_TASK_RULE_FILE"
fi

echo "🚀 Starting deployment process..."

# Build and package the Lambda function
echo "📦 Building and packaging Lambda function..."
cd TogglTimesheet.Api
dotnet restore
dotnet publish -c Release
dotnet lambda package --configuration Release --framework net8.0 --output-package bin/Release/net8.0/publish/package.zip

# Apply Terraform changes
echo "🏗️ Applying Terraform changes..."
cd ../terraform
terraform init
terraform apply -auto-approve

echo "✅ Deployment completed successfully!"

# Get the function URL
echo "🌐 Lambda Function URL:"
terraform output -raw function_url
