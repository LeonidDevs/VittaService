using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Models;

namespace Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly VittaDbContext _dbContext;
        public AuthController(VittaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{UserLogin}")]
        public async Task<ActionResult<User>> AuthUser(string UserLogin)
        {
            if (string.IsNullOrEmpty(UserLogin))
            {
                return BadRequest("Login cannot be empty");
            }
            var user = await _dbContext.Users
                .Where(u => u.UserLogin == UserLogin)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    UserLogin = u.UserLogin,
                    UserName = u.UserName
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
