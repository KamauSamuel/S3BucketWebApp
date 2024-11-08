using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace S3BucketwithAuth.Services;

public class BucketService : IBucketServices
{
    private readonly IAmazonS3 _amazonS3;
    private readonly IOptions<S3BucketSettings> _s3Settings;
    public BucketService(IAmazonS3 amazonS3, IOptions<S3BucketSettings> s3Settings)
    {
        _amazonS3 = amazonS3;
        _s3Settings = s3Settings;
    }
    public async Task<string> documenttitleAsync(string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{key}"
        };
        var response = await _amazonS3.GetObjectAsync(request);
        return response.Metadata["file-name"];
    }
}
