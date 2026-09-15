using MongoDB.Bson.Serialization.Attributes;

namespace MoshEngine.Candy.AspNet.Auth;

public class Role
{
    [BsonId]
    public string Name { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
