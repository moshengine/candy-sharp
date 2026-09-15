using Discord;

namespace MoshEngine.Candy.Discord;

public static class EmbedBuilderExtensions
{
    public static class Colors
    {
        public static readonly Color Success = new(67, 181, 129);
        public static readonly Color Error = new(240, 71, 71);
        public static readonly Color Warning = new(255, 184, 0);
        public static readonly Color Info = new(52, 152, 219);
        public static readonly Color Default = new(149, 165, 166);
        
        public static readonly Color DiscordBlurple = new(88, 101, 242);
        public static readonly Color DiscordGreen = new(87, 242, 135);
        public static readonly Color DiscordYellow = new(254, 231, 92);
        public static readonly Color DiscordFuchsia = new(235, 69, 158);
        public static readonly Color DiscordRed = new(237, 66, 69);
    }

    public static EmbedBuilder WithSuccessColor(this EmbedBuilder builder)
    {
        return builder.WithColor(Colors.Success);
    }

    public static EmbedBuilder WithErrorColor(this EmbedBuilder builder)
    {
        return builder.WithColor(Colors.Error);
    }

    public static EmbedBuilder WithWarningColor(this EmbedBuilder builder)
    {
        return builder.WithColor(Colors.Warning);
    }

    public static EmbedBuilder WithInfoColor(this EmbedBuilder builder)
    {
        return builder.WithColor(Colors.Info);
    }

    public static EmbedBuilder WithDefaultColor(this EmbedBuilder builder)
    {
        return builder.WithColor(Colors.Default);
    }
}

