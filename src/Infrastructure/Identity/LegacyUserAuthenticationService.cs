using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ebr_powerbi.Infrastructure.Identity;

public class LegacyUserAuthenticationService : ILegacyUserAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<object> _passwordHasherV2;
    private readonly PasswordHasher<object> _passwordHasherV3;

    public LegacyUserAuthenticationService(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasherV2 = new PasswordHasher<object>(Options.Create(
            new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
        _passwordHasherV3 = new PasswordHasher<object>(Options.Create(
            new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3 }));
    }

    public async Task<LegacyUserCredentialsResult?> ValidateCredentialsAsync(
        string emailOrUserName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var login = (emailOrUserName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var normalized = login.ToLowerInvariant();
        var user = await _context.AspNetUsers
            .Where(u =>
                (u.Email != null && u.Email.ToLower() == normalized) ||
                (u.UserName != null && u.UserName.ToLower() == normalized))
            .Select(u => new { u.Id, u.Email, u.UserName, u.PasswordHash })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var verificationV3 = _passwordHasherV3.VerifyHashedPassword(new object(), user.PasswordHash, password);
        var verificationV2 = _passwordHasherV2.VerifyHashedPassword(new object(), user.PasswordHash, password);
        var isValid =
            verificationV3 != PasswordVerificationResult.Failed ||
            verificationV2 != PasswordVerificationResult.Failed;
        if (!isValid)
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
