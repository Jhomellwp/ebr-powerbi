namespace ebr_powerbi.Application.Common.Interfaces;

public interface ILegacyUserAuthenticationService
{
    Task<LegacyUserCredentialsResult?> ValidateCredentialsAsync(
        string emailOrUserName,
        string password,
        CancellationToken cancellationToken = default);
}

public sealed record LegacyUserCredentialsResult(
    string Id,
    string Email,
    string UserName,
    IReadOnlyList<string> Roles);
