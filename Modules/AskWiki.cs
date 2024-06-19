using System.Text;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace bozo_ai_discord_qna.Modules;

public class AskWiki : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<AskWiki> _logger;
    private readonly Configuration _options;

    public AskWiki(IOptions<Configuration> options, ILogger<AskWiki> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [SlashCommand("wiki", "Ask Berani Digital Wiki for information.", runMode: RunMode.Async)]
    public async Task Wiki(string question)
    {
        if (_options.AskWikiURL == null)
        {
            await RespondAsync("Wiki URL is not set.");
            return;
        }

        await DeferAsync();
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, this._options.AskWikiURL);
        var contentDict = new Dictionary<string, string>();
        contentDict.Add("input", question);
        var content = JsonConvert.SerializeObject(contentDict);
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
        if (!responseDict?.ContainsKey("output") ?? true)
        {
            await FollowupAsync("I'm sorry, I don't have an answer for that.");
            return;
        }

        var answer = responseDict["output"];
        await FollowupAsync(answer);
    }
}
