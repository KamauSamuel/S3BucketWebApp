using S3BucketwithAuth.Infrastructure;
using S3BucketwithAuth.Models;
using S3BucketwithAuth.Models.Contracts;
using S3BucketwithAuth.Models.Enums;

namespace S3BucketwithAuth.Services;

public class UserService : IUserService
{
    private readonly ITokenProvider _tokenProvider;
    private List<User> _users = new List<User>
    {
        new User {Id = 1, FirstName = "Test", LastName = "Test", Username = "test1", Password = "test1", dept = Departments.Finance},
        new User {Id = 2, FirstName = "UserTwo", LastName = "UserTwo", Username = "test2", Password = "test2", dept = Departments.HR}
    };
    public UserService(ITokenProvider tokenprovider)
    {
        _tokenProvider = tokenprovider;
    }
    public AuthenticationResponse Authenticate(AuthenticateRequest authencticateRequest)
    {
        var user = _users.SingleOrDefault(x => x.Username == authencticateRequest.Username && x.Password == authencticateRequest.Password);
        if(user == null)
        {
            throw new Exception("The user was not found");
        }
        string token = _tokenProvider.CreateToken(user);
        return new AuthenticationResponse(user.Id, user.FirstName, user.LastName, user.Username, token);   
    }

    public IEnumerable<User> GetAll()
    {
        return _users;
    }

    public User? GetById(int Id)
    {
        return _users.FirstOrDefault(u => u.Id == Id);
    }
}
