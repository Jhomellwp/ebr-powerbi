using ebr_powerbi.Application.Common.Interfaces;

namespace ebr_powerbi.Application.Users.Queries.GetCurrentUserInfo;

public sealed record GetCurrentUserInfoQuery : IRequest<CurrentUserInfoVm>;

public sealed record CurrentUserInfoVm(
    bool IsAuthenticated,
    string? UserId,
    string? Email,
    string? UserName,
    IReadOnlyList<string> Roles);

public class GetCurrentUserInfoQueryHandler : IRequestHandler<GetCurrentUserInfoQuery, CurrentUserInfoVm>
{
    private readonly IUser _user;

    public GetCurrentUserInfoQueryHandler(IUser user)
    {
        _user = user;
    }

    public Task<CurrentUserInfoVm> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Task.FromResult(new CurrentUserInfoVm(false, null, null, null, Array.Empty<string>()));
        }

        var roles = _user.Roles ?? new List<string>();
        return Task.FromResult(new CurrentUserInfoVm(
            true,
            _user.Id,
            _user.Email,
            _user.UserName,
            roles));
    }
}
