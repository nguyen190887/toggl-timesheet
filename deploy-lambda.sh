#!/bin/bash

# Exit on any error
set -e

# Check if required variables are provided
if [ -z "$TF_VAR_domain_name" ]; then
    echo "❌ Error: TF_VAR_domain_name environment variable must be set"
    echo "Example usage:"
    echo "TF_VAR_domain_name=your-domain.com ./deploy-lambda.sh"
    exit 1
fi


if [ -z "$TF_BACKEND_BUCKET" ]; then
    echo "❌ Error: TF_BACKEND_BUCKET environment variable must be set"
    exit 1
fi

if [ -z "$TF_BACKEND_KEY" ]; then
    echo "❌ Error: TF_BACKEND_KEY environment variable must be set"
    exit 1
fi

if [ -z "$TOGGL_API_TOKEN" ]; then
    echo "❌ Error: TOGGL_API_TOKEN environment variable must be set"
    exit 1
fi

if [ -z "$TOGGL_WORKSPACE_ID" ]; then
    echo "❌ Error: TOGGL_WORKSPACE_ID environment variable must be set"
    exit 1
fi

echo "🚀 Starting Lambda deployment process..."

# Build and package the Lambda function
echo "📦 Building and packaging Lambda function..."
cd TogglTimesheet.Api
dotnet publish -c Release

# Apply Terraform changes for backend stack
echo "🏗️ Applying Terraform backend changes..."
cd ../terraform/backend
cp ../rootvars.tfvars .
terraform init \
    -backend-config="bucket=${TF_BACKEND_BUCKET}" \
    -backend-config="key=${TF_BACKEND_KEY}/backend.tfstate" \
    -backend-config="region=us-east-1"
TF_VAR_toggl_api_token=$TOGGL_API_TOKEN \
TF_VAR_toggl_workspace_id=$TOGGL_WORKSPACE_ID \
terraform apply -auto-approve

echo "✅ Lambda deployment completed successfully!"

# Get the API URL
echo "🌐 API URL:"
terraform output -raw function_url
