output "function_url" {
  description = "Lambda function URL"
  value       = aws_lambda_function_url.api_url.function_url
}

output "function_name" {
  description = "Lambda function name"
  value       = aws_lambda_function.api.function_name
}
