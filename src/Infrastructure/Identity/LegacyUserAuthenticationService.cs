using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ebr_powerbi.Infrastructure.Identity;

public class LegacyUserAuthenticationService : ILegacyUserAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<object> _passwordHasher;

    public LegacyUserAuthenticationService(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<object>(Options.Create(
            new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
    }

    public async Task<LegacyUserCredentialsResult?> ValidateCredentialsAsync(
        string emailOrUserName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.AspNetUsers
            .Where(u => u.Email == emailOrUserName || u.UserName == emailOrUserName)
            .Select(u => new { u.Id, u.Email, u.UserName, u.PasswordHash })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(new object(), user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var roles = await (
            from ur in _context.AspNetUserRoles
            join r in _context.AspNetRoles on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name
        ).ToListAsync(cancellationToken);

        return new LegacyUserCredentialsResult(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            roles);
    }
}
