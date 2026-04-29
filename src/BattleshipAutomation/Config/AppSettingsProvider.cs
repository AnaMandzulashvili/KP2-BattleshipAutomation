using Microsoft.Extensions.Configuration;

namespace BattleshipAutomation.Config;

/// <summary>
/// Loads typed configuration from gameSettings.json once at startup.
/// </summary>
public static class AppSettingsProvider
{
    private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
        .AddJsonFile("gameSettings.json", optional: false, reloadOnChange: false)
        .Build();

    public static GameSettings GameSettings { get; } =
        Config.GetSection("GameSettings").Get<GameSettings>()
        ?? throw new InvalidOperationException(
            "Failed to bind 'GameSettings' section from gameSettings.json.");
}
