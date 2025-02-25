variable "aws_region" {
  description = "AWS region for the infrastructure"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
  default     = "dev"
}

variable "lambda_package_path" {
  description = "Path to the Lambda function package"
  type        = string
  default     = "../TogglTimesheet.Api/bin/Release/net8.0/publish/package.zip"
}
