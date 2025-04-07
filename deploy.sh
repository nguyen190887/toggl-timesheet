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

# Check if domain name is provided
if [ -z "$TF_VAR_domain_name" ]; then
    echo "❌ Error: TF_VAR_domain_name environment variable must be set"
    echo "Example usage:"
    echo "TF_VAR_domain_name=your-domain.com ./deploy.sh"
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

# Deploy Lambda backend first
./deploy-lambda.sh

# Then deploy frontend
./deploy-frontend.sh

echo "✨ Deployment completed successfully!"
