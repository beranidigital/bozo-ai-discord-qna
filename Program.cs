
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


var builder = Host.CreateApplicationBuilder(args);
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

builder.Configuration.SetBasePath(currentDirectory);
builder.Configuration.AddEnvironmentVariables();

bool foundAppSetting = false;
foreach (var possibleAppSettingLocation in possibleAppSettingLocations)
{
    if (File.Exists(possibleAppSettingLocation))
    {
        foundAppSetting = true;
        builder.Configuration.AddJsonFile(possibleAppSettingLocation, false, true);
        break;
    }
}

if (!foundAppSetting)
{
    throw new FileNotFoundException("Could not find appsettings.json");
}

builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<Configuration>(
    builder.Configuration.GetSection(Configuration.SectionName)); // Add the EqueConfiguration to services


builder.Services.AddSingleton(new DiscordSocketClient(configDiscord)); // Add the discord client to services
builder.Services.AddSingleton<InteractionService>(); // Add the interaction service to services

// Register any class that ends with "Service" as a service
builder.Services.RegisterAssemblyPublicNonGenericClasses()
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces();

var host = builder.Build();


await host.RunAsync();
