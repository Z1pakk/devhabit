using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext dbContext,
    TokenProvider tokenProvider
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokenDto>> Register(
        [FromBody] RegisterUserDto registerUserDto
    )
    {
        await using IDbContextTransaction transaction =
            await identityDbContext.Database.BeginTransactionAsync();
        dbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await dbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        IdentityUser identityUser = new()
        {
            UserName = registerUserDto.Email,
            Email = registerUserDto.Email,
        };

        IdentityResult result = await userManager.CreateAsync(
            identityUser,
            registerUserDto.Password
        );
        if (!result.Succeeded)
        {
            var extensions = new Dictionary<string, object?>()
            {
                ["errors"] = result.Errors.ToDictionary(e => e.Code, e => e.Description),
            };

            return Problem(
                detail: "Unable to register user",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extensions
            );
        }

        User user = registerUserDto.ToEntity();
        user.IdentityId = identityUser.Id;

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        AccessTokenDto accessTokens = tokenProvider.Create(
            new TokenRequest(identityUser.Id, identityUser.Email)
        );

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = accessTokens.RefreshTokenExpiration,
        };

        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenDto>> Login([FromBody] LoginUserDto loginUserDto)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (identityUser is null)
        {
            return Problem(
                detail: "Invalid email or password",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        bool isPasswordValid = await userManager.CheckPasswordAsync(
            identityUser,
            loginUserDto.Password
        );
        if (!isPasswordValid)
        {
            return Problem(
                detail: "Invalid email or password",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        AccessTokenDto accessTokens = tokenProvider.Create(
            new TokenRequest(identityUser.Id, identityUser.Email!)
        );

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = accessTokens.RefreshTokenExpiration,
        };

        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult<AccessTokenDto>> Refresh(
        [FromBody] RefreshTokenDto refreshTokenRequest
    )
    {
        RefreshToken? refreshToken = await identityDbContext
            .RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenRequest.RefreshToken);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Problem(
                detail: "Invalid or expired refresh token",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        IdentityUser? identityUser = await userManager.FindByIdAsync(refreshToken.UserId);
        if (identityUser is null)
        {
            return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);
        }

        AccessTokenDto accessTokens = tokenProvider.Create(
            new TokenRequest(identityUser.Id, identityUser.Email!)
        );

        // Update the existing refresh token
        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUtc = accessTokens.RefreshTokenExpiration;

        identityDbContext.RefreshTokens.Update(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }
}
