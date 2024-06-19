using System.Globalization;
using System.Reflection;
using bozo_ai_discord_qna.Utility;
using CacheTower;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace bozo_ai_discord_qna.Modules;

public class DevModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<DevModule> _logger;
    private readonly Configuration _options;

    public DevModule(IOptions<Configuration> options, ILogger<DevModule> logger)
    {
        _options = options.Value;
        _logger = logger;
    }


    private static DateTime GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string buildVersionMetadataPrefix = "+build";
        var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();
        if (attribute?.DateTime == null) return default;
        return attribute.DateTime;
    }

    private async Task Info()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Developer Information")
            .AddField("Environment.MachineName", Environment.MachineName)
            .AddField("Environment.UserName", Environment.UserName)
            .AddField("Environment.ProcessId", Environment.ProcessId)
            .AddField("Build Date", GetBuildDate().ToString("yyyy-MM-dd HH:mm:ss"))
            .Build();
        await FollowupAsync(embed: embed);
    }

    private async Task CleanupCache()
    {
        await FollowupAsync("Cache cleaned up.");
    }

    [SlashCommand("dev", "Bot developer only.", runMode: RunMode.Async)]
    public async Task Dev(string command)
    {
        if (!_options.DevIds.Contains(Context.User.Id + ""))
        {
            await RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Unauthorized")
                .WithDescription("You are not authorized to use this command.")
                .WithColor(Color.Red)
                .Build());
            return;
        }

        await DeferAsync();
        try
        {
            switch (command)
            {
                case "pwd":
                    await FollowupAsync(Environment.CurrentDirectory);
                    break;
                case "exit":
                    await FollowupAsync("Exiting...");
                    Environment.Exit(0);
                    break;
                case "info":
                    await Info();
                    break;
                case "cleanup-cache":
                    await CleanupCache();
                    break;

                default:
                    await FollowupAsync("Unknown command.");
                    break;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred");
            await FollowupAsync($"An error occurred: {e.Message}");
        }
    }
}
