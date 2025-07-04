#!/bin/bash

# Exit on any error
set -e

# Check if required variables are provided
if [ -z "$TF_VAR_domain_name" ]; then
    echo "❌ Error: TF_VAR_domain_name environment variable must be set"
    echo "Example usage:"
    echo "TF_VAR_domain_name=your-domain.com ./deploy-frontend.sh"
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

echo "🚀 Starting frontend deployment process..."

# Build the frontend application
echo "📦 Building frontend application..."
cd TogglTimesheet.App
pnpm install
pnpm build

# Apply Terraform changes for frontend stack
echo "🏗️ Applying Terraform frontend changes..."
cd ../terraform/frontend
cp ../rootvars.tfvars .
terraform init \
    -backend-config="bucket=${TF_BACKEND_BUCKET}" \
    -backend-config="key=${TF_BACKEND_KEY}/frontend.tfstate" \
    -backend-config="region=us-east-1"
terraform apply -auto-approve

# Get the S3 bucket name from Terraform output
S3_BUCKET=$(terraform output -raw s3_bucket_name)
CLOUDFRONT_DIST_ID=$(terraform output -raw cloudfront_distribution_id)

# Sync the build files to S3
echo "📤 Syncing build files to S3..."
cd ../../TogglTimesheet.App/dist
aws s3 sync . "s3://$S3_BUCKET" --delete

# Invalidate CloudFront cache
echo "🔄 Invalidating CloudFront cache..."
aws cloudfront create-invalidation --distribution-id "$CLOUDFRONT_DIST_ID" --paths "/*"

echo "✅ Frontend deployment completed successfully!"

# Get the website URL
echo "🌐 Website URL:"
cd ../../terraform/frontend
terraform output -raw website_url
