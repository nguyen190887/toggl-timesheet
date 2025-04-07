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

variable "route53_zone_id" {
  type        = string
  description = "Route 53 hosted zone ID for DNS records"
}
