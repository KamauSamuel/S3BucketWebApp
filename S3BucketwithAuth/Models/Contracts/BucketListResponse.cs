namespace S3BucketwithAuth.Models.Contracts;

public record BucketListResponse(string key, string title, double filesize, DateTime lastmodified);

