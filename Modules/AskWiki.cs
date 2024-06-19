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
        var input = new Input
        {
            InputInput = new InputClass
            {
                Input = question
            }
        };
        var content = JsonConvert.SerializeObject(input);
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Output>(responseContent);
        if (responseDict == null)
        {
            await FollowupAsync("No response from the server.");
            return;
        }

        await FollowupAsync(responseDict.OutputOutput.Output);
    }

    public partial class Input
    {
        [JsonProperty("input")] public InputClass InputInput { get; set; }
    }

    public partial class InputClass
    {
        [JsonProperty("input")] public string Input { get; set; }
    }

    public partial class Output
    {
        [JsonProperty("output")] public OutputClass OutputOutput { get; set; }

        [JsonProperty("metadata")] public Metadata Metadata { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("run_id")] public Guid RunId { get; set; }

        [JsonProperty("feedback_tokens")] public object[] FeedbackTokens { get; set; }
    }

    public partial class OutputClass
    {
        [JsonProperty("output")] public string Output { get; set; }
    }
}
