using S3BucketwithAuth.Models;

namespace S3BucketwithAuth.Infrastructure;

public interface ITokenProvider
{
    string CreateToken(User user);
}
