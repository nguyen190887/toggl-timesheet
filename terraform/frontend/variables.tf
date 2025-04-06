variable "aws_region" {
  description = "AWS region for the infrastructure"
  type        = string
}

variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "domain_name" {
  description = "Domain name for the web application"
  type        = string
}

variable "web_app_build_path" {
  description = "Path to the web application build directory"
  type        = string
  default     = "../../TogglTimesheet.App/dist"
}
