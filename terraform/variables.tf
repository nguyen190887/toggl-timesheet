variable "aws_region" {
  description = "AWS region for the infrastructure"
  type        = string
}

variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "lambda_package_path" {
  description = "Path to the Lambda function package"
  type        = string
}

variable "toggl_api_token" {
  description = "Toggl API token"
  type        = string
  sensitive   = true
}

variable "toggl_workspace_id" {
  description = "Toggl workspace ID"
  type        = string
  sensitive   = true
}

variable "toggl_task_rule_file" {
  description = "Path to the task rules configuration file"
  type        = string
  default     = "Config/taskRules.json"
}
