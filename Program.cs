
using bozo_ai_discord_qna;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;

var configDiscord = new DiscordSocketConfig
{
    AlwaysDownloadUsers = true,
    GatewayIntents = GatewayIntents.MessageContent
                     | GatewayIntents.DirectMessages
                     | GatewayIntents.DirectMessageReactions
                     | GatewayIntents.DirectMessageTyping
                     | GatewayIntents.GuildMembers
                     | GatewayIntents.GuildMessages
                     | GatewayIntents.Guilds
                     | GatewayIntents.GuildMessageReactions
                     | GatewayIntents.GuildMessageTyping
};

var currentDirectory = Directory.GetCurrentDirectory();
var possibleAppSettingLocations = new[]
{
    Path.Combine(currentDirectory, "appsettings.json"),
    Path.Combine(currentDirectory, "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "..", "..", "appsettings.json"),
};




var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();


foreach (var possibleAppSettingLocation in possibleAppSettingLocations)
{
    if (File.Exists(possibleAppSettingLocation))
    {
        builder.Configuration.AddJsonFile(possibleAppSettingLocation, false, true);
        break;
    }
}

builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.Configure<Configuration>(builder.Configuration);


builder.Services.AddSingleton(new DiscordSocketClient(configDiscord)); // Add the discord client to services
builder.Services.AddSingleton<InteractionService>(); // Add the interaction service to services

// Register any class that ends with "Service" as a service
builder.Services.RegisterAssemblyPublicNonGenericClasses()
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces();

var host = builder.Build();


await host.RunAsync();
