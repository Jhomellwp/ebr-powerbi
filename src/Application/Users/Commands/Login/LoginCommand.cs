using ebr_powerbi.Application.Common.Interfaces;

namespace ebr_powerbi.Application.Users.Commands.Login;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<LoginCommandResult>;

public sealed record LoginCommandResult(
    bool Succeeded,
    string? UserId,
    string? Email,
    string? UserName,
    IReadOnlyList<string> Roles);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty();
        RuleFor(v => v.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly ILegacyUserAuthenticationService _legacyAuth;

    public LoginCommandHandler(ILegacyUserAuthenticationService legacyAuth)
    {
        _legacyAuth = legacyAuth;
    }

    public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _legacyAuth.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);

        if (user is null)
        {
            return new LoginCommandResult(false, null, null, null, Array.Empty<string>());
        }

        return new LoginCommandResult(true, user.Id, user.Email, user.UserName, user.Roles);
    }
}
