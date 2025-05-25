terraform {
  backend "s3" {}

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# IAM role for Lambda
resource "aws_iam_role" "lambda_role" {
  name = "toggl-timesheet-lambda-role-${var.environment}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

# IAM policy for Lambda
resource "aws_iam_role_policy" "lambda_policy" {
  name = "toggl-timesheet-lambda-policy-${var.environment}"
  role = aws_iam_role.lambda_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = ["arn:aws:logs:*:*:*"]
      }
    ]
  })
}

# Lambda function
resource "aws_lambda_function" "api" {
  filename         = var.lambda_package_path
  function_name    = "toggl-timesheet-api-${var.environment}"
  role            = aws_iam_role.lambda_role.arn
  handler         = "TogglTimesheet.Api::TogglTimesheet.Api.LambdaEntryPoint::FunctionHandlerAsync"
  runtime         = "dotnet8"
  memory_size     = 256
  timeout         = 30
  architectures   = ["x86_64"]
  source_code_hash = filebase64sha256(var.lambda_package_path)

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = var.environment
      Toggl__ApiToken       = var.toggl_api_token
      Toggl__WorkspaceId    = var.toggl_workspace_id
      Toggl__TaskRuleFile   = var.toggl_task_rule_file
    }
  }
}

# Lambda Function URL
resource "aws_lambda_function_url" "api_url" {
  function_name      = aws_lambda_function.api.function_name
  authorization_type = "NONE"

  cors {
    allow_credentials = true
    allow_origins     = ["*"]
    allow_methods     = ["*"]
    allow_headers     = ["*"]
    expose_headers    = ["*"]
    max_age          = 86400
  }

  lifecycle {
    replace_triggered_by = [aws_lambda_function.api.function_name]
  }
}
