using System.Text;

namespace Candy.Discord;

public static class MessageFormatter
{
    public static class Emoji
    {
        public const string Success = "✅";
        public const string Error = "❌";
        public const string Warning = "⚠️";
        public const string Info = "ℹ️";
        public const string Loading = "⏳";
        public const string CheckMark = "✓";
        public const string CrossMark = "✗";
        public const string Question = "❓";
        public const string Exclamation = "❗";
        public const string Star = "⭐";
        public const string Fire = "🔥";
        public const string Rocket = "🚀";
    }

    public static string Bold(string text) => $"**{text}**";
    
    public static string Italic(string text) => $"*{text}*";
    
    public static string Underline(string text) => $"__{text}__";
    
    public static string Strikethrough(string text) => $"~~{text}~~";
    
    public static string Code(string text) => $"`{text}`";
    
    public static string CodeBlock(string text, string language = "") => 
        $"```{language}\n{text}\n```";
    
    public static string Quote(string text) => $"> {text}";
    
    public static string QuoteBlock(string text)
    {
        var lines = text.Split('\n');
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine($"> {line}");
        }
        return sb.ToString().TrimEnd();
    }
    
    public static string Mention(ulong userId) => $"<@{userId}>";
    
    public static string MentionChannel(ulong channelId) => $"<#{channelId}>";
    
    public static string MentionRole(ulong roleId) => $"<@&{roleId}>";
    
    public static string Timestamp(DateTimeOffset timestamp, TimestampStyle style = TimestampStyle.ShortDateTime) =>
        $"<t:{timestamp.ToUnixTimeSeconds()}:{GetTimestampStyleChar(style)}>";
    
    public static string Link(string text, string url) => $"[{text}]({url})";
    
    public static string Spoiler(string text) => $"||{text}||";

    public static string Truncate(string text, int maxLength, string suffix = "...")
    {
        if (text.Length <= maxLength)
            return text;
        
        return text[..(maxLength - suffix.Length)] + suffix;
    }

    private static char GetTimestampStyleChar(TimestampStyle style) => style switch
    {
        TimestampStyle.ShortTime => 't',
        TimestampStyle.LongTime => 'T',
        TimestampStyle.ShortDate => 'd',
        TimestampStyle.LongDate => 'D',
        TimestampStyle.ShortDateTime => 'f',
        TimestampStyle.LongDateTime => 'F',
        TimestampStyle.Relative => 'R',
        _ => 'f'
    };
}

public enum TimestampStyle
{
    ShortTime,
    LongTime,
    ShortDate,
    LongDate,
    ShortDateTime,
    LongDateTime,
    Relative
}

