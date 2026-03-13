namespace Candy.AspNet.Auth;

public interface IUser
{
    string Id { get; }
    string? DiscordId { get; }
    string Email { get; }
    string Username { get; }
    string? AvatarUrl { get; }
    string Role { get; }
    List<string>? PermissionGrants { get; }
    List<string>? PermissionDenies { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; set; }
}
