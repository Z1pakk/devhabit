using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        UserDto? user = await dbContext
            .Users.Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }
}
