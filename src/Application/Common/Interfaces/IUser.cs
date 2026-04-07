namespace ebr_powerbi.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    string? Email { get; }
    string? UserName { get; }
    List<string>? Roles { get; }
}
