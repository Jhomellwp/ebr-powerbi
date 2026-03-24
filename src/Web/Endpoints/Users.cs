using System.Security.Claims;
using ebr_powerbi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ebr_powerbi.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(Info, "info");
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    public sealed record LoginRequest(string Email, string Password);

    [EndpointSummary("Get current user info")]
    [EndpointDescription("Returns basic details for the currently authenticated user.")]
    public static IResult Info(HttpContext httpContext)
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            return TypedResults.Ok(new { isAuthenticated = false });
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = httpContext.User.FindFirstValue(ClaimTypes.Email);
        var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();

        return TypedResults.Ok(new
        {
            isAuthenticated = true,
            userId,
            email,
            roles
        });
    }

    [EndpointSummary("Login with EBR identity user")]
    [EndpointDescription("Authenticates using the shared AspNetUsers table and issues an auth cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Login(
        HttpContext httpContext,
        ApplicationDbContext dbContext,
        LoginRequest request)
    {
        var user = await dbContext.AspNetUsers
            .Where(u => u.Email == request.Email || u.UserName == request.Email)
            .Select(u => new { u.Id, u.Email, u.UserName, u.PasswordHash })
            .FirstOrDefaultAsync();

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return TypedResults.Unauthorized();
        }

        var hasher = new PasswordHasher<object>(Microsoft.Extensions.Options.Options.Create(
            new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
        var verification = hasher.VerifyHashedPassword(new object(), user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return TypedResults.Unauthorized();
        }

        var roles = await (
            from ur in dbContext.AspNetUserRoles
            join r in dbContext.AspNetRoles on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name
        ).ToListAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return TypedResults.Ok();
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user by clearing the authentication cookie.")]
    public static async Task<Ok> Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return TypedResults.Ok();
    }
}
