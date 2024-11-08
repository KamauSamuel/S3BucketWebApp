using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using S3BucketwithAuth.Models;
using S3BucketwithAuth.Models.Contracts;
using S3BucketwithAuth.Services;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace S3BucketwithAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        public UsersController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;

        }
        // GET: api/<UsersController>
        [Authorize]
        [HttpGet("GetLoggedOnUserDepartment")]
        public ActionResult<string> GetDepartment()
        {
           //var username = User.Claims.Where( u => u.Type == ClaimTypes.Name)
           //     .Select(u => u.Value);
            return Ok(_tokenService.GetUserDepartment());
        }

        // GET api/<UsersController>/5
        [Authorize]
        [HttpGet("{id:int}")]
        public ActionResult<User> Get(int id)
        {
            var us= _userService.GetById(id);
            return Ok(us);
        }

        // POST api/<UsersController>
        [HttpPost("login")]
        public IActionResult LoginRequest([FromBody] AuthenticateRequest request)
        {
            var resutlt = _userService.Authenticate(request);
            return Ok(resutlt);
        }
    }
}
