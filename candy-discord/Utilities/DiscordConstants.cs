using Discord;

namespace MoshEngine.Candy.Discord;

public static class DiscordConstants
{
    public static class Limits
    {
        public const int MessageContent = 2000;
        public const int EmbedTitle = 256;
        public const int EmbedDescription = 4096;
        public const int EmbedFieldName = 256;
        public const int EmbedFieldValue = 1024;
        public const int EmbedFields = 25;
        public const int EmbedFooter = 2048;
        public const int EmbedAuthor = 256;
        public const int EmbedTotal = 6000;
        
        public const int CommandName = 32;
        public const int CommandDescription = 100;
        public const int ChoiceName = 100;
        public const int ChoiceValue = 100;
        
        public const int ButtonLabel = 80;
        public const int SelectMenuOptions = 25;
        public const int SelectMenuPlaceholder = 150;
        
        public const int BulkDeleteMessages = 100;
        public const int BulkDeleteDays = 14;
    }

    public static class Permissions
    {
        public static readonly GuildPermission Administrator = GuildPermission.Administrator;
        public static readonly GuildPermission ModeratorBasic = 
            GuildPermission.ManageMessages | 
            GuildPermission.ManageChannels |
            GuildPermission.KickMembers |
            GuildPermission.BanMembers;
        
        public static readonly GuildPermission BotBasic = 
            GuildPermission.ViewChannel |
            GuildPermission.SendMessages |
            GuildPermission.EmbedLinks |
            GuildPermission.AttachFiles |
            GuildPermission.ReadMessageHistory |
            GuildPermission.AddReactions;
    }

    public static class RateLimits
    {
        public const int MessagesPerSecond = 5;
        public const int DeleteMessagesPerSecond = 5;
        public const int BulkDeletesPerSecond = 1;
    }
}

