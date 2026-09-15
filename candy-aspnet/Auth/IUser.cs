namespace Candy.AspNet.Auth;

public interface IUser
{
    string Id { get; }
    string? DiscordId => null;
    string Email { get; }
    string Username { get; }
    string? AvatarUrl { get; }

    /// <summary>Role-based permission system (mosh-engine).</summary>
    string Role => string.Empty;

    /// <summary>Role-based permission system (mosh-engine).</summary>
    List<string>? PermissionGrants => null;

    /// <summary>Role-based permission system (mosh-engine).</summary>
    List<string>? PermissionDenies => null;

    /// <summary>Simple per-user permission system (Tomoni).</summary>
    List<string>? Permissions => null;

    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; set; }
}
