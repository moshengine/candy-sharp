using MongoDB.Driver;

namespace MoshEngine.Candy.AspNet.Auth;

public class DatabaseRolePermissionDefaults : IRolePermissionDefaults
{
    private readonly IMongoCollection<Role> _roles;
    private Dictionary<string, Role> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public DatabaseRolePermissionDefaults(IMongoDatabase database)
    {
        _roles = database.GetCollection<Role>("roles");
    }

    public IReadOnlySet<string> ForRole(string role)
    {
        EnsureLoaded();
        return _cache.TryGetValue(role, out var r)
            ? new HashSet<string>(r.Permissions, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
    }

    public IReadOnlyList<Role> GetAllRoles()
    {
        EnsureLoaded();
        return _cache.Values.OrderBy(r => r.SortOrder).ToList();
    }

    public Role? GetDefaultRole()
    {
        EnsureLoaded();
        return _cache.Values.FirstOrDefault(r => r.IsDefault);
    }

    public async Task ReloadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var roles = await _roles.Find(_ => true).ToListAsync();
            _cache = roles.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
            _loaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        _lock.Wait();
        try
        {
            if (_loaded) return;
            var roles = _roles.Find(_ => true).ToList();
            _cache = roles.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
            _loaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
