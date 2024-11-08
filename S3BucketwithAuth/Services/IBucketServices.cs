namespace S3BucketwithAuth.Services
{
    public interface IBucketServices
    {
        Task<string> documenttitleAsync(string key);
    }
}
