using System.Security.Claims;
using ebr_powerbi.Application.Users.Commands.Login;
using ebr_powerbi.Application.Users.Queries.GetCurrentUserInfo;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;

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
    public static async Task<IResult> Info(ISender sender)
    {
        var vm = await sender.Send(new GetCurrentUserInfoQuery());
        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Login with EBR identity user")]
    [EndpointDescription("Authenticates using the shared AspNetUsers table and issues an auth cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Login(
        HttpContext httpContext,
        ISender sender,
        LoginRequest request)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password));

        if (!result.Succeeded || result.UserId is null)
        {
            return TypedResults.Unauthorized();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId),
            new(ClaimTypes.Name, result.UserName ?? result.Email ?? string.Empty),
            new(ClaimTypes.Email, result.Email ?? string.Empty)
        };
        claims.AddRange(result.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
