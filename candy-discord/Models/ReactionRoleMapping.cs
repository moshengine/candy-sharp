using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Candy.Discord;

public class ReactionRoleMapping
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("messageId")]
    public ulong MessageId { get; set; }

    [BsonElement("emoji")]
    public string Emoji { get; set; } = string.Empty;

    [BsonElement("roleId")]
    public ulong RoleId { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("whitelistedUserIds")]
    public List<ulong> WhitelistedUserIds { get; set; } = new();
}

