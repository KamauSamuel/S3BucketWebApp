using S3BucketwithAuth.Models.Enums;
using System.Text.Json.Serialization;

namespace S3BucketwithAuth.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public Departments dept { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
}
