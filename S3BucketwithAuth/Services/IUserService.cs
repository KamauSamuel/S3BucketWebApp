using S3BucketwithAuth.Models;
using S3BucketwithAuth.Models.Contracts;

namespace S3BucketwithAuth.Services;

public interface IUserService
{
    AuthenticationResponse Authenticate (AuthenticateRequest authencticateRequest);
    IEnumerable<User> GetAll();
    User? GetById(int Id);
}
