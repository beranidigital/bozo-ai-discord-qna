namespace bozo_ai_discord_qna;

public class Configuration
{
    public string DiscordToken { get; set; }
    public static string SectionName { get; set; } = "";
    public string[] DevIds { get; set; } = new string[0];
}
