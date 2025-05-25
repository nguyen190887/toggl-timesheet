output "cloudfront_distribution_id" {
  description = "The ID of the CloudFront distribution"
  value       = aws_cloudfront_distribution.website.id
}

output "cloudfront_domain_name" {
  description = "The domain name of the CloudFront distribution"
  value       = aws_cloudfront_distribution.website.domain_name
}

output "s3_bucket_name" {
  description = "The name of the S3 bucket hosting the website"
  value       = aws_s3_bucket.website.id
}

output "website_url" {
  description = "The URL of the website"
  value       = "https://${var.domain_name}"
}
