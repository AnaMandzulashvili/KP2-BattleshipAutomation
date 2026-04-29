namespace BattleshipAutomation.Config;

/// <summary>Game constants loaded from gameSettings.json.
/// Adjust this file to adapt the framework to a different game variant.</summary>
public class GameSettings
{
    public string              BaseUrl           { get; init; } = string.Empty;
    public int                 BoardSize         { get; init; }
    public IReadOnlyList<int>  Fleet             { get; init; } = Array.Empty<int>();
    public int                 MinRandomiseClicks { get; init; }
    public int                 MaxRandomiseClicks { get; init; }
    public TimeoutSettings     Timeouts          { get; init; } = new();
    public StatusMessages      StatusMessages    { get; init; } = new();
}
