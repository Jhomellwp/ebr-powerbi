using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Models;
using ebr_powerbi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ebr_powerbi.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;

    public IdentityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        return await _context.AspNetUsers
            .Where(u => u.Id == userId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync();
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        await Task.CompletedTask;
        return (Result.Failure(["User provisioning is managed by EBR."]), string.Empty);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        return await (
            from ur in _context.AspNetUserRoles
            join r in _context.AspNetRoles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Name == role
            select ur.UserId
        ).AnyAsync();
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        // Policy evaluation from legacy DB is application-specific.
        // Keep this conservative until policies are explicitly defined.
        await Task.CompletedTask;
        return false;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        await Task.CompletedTask;
        return Result.Failure(["User provisioning is managed by EBR."]);
    }
}
