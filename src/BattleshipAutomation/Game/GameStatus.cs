using BattleshipAutomation.Config;

namespace BattleshipAutomation.Game;

/// <summary>
/// Game notification texts as they appear in the English UI, and helpers that classify them.
/// These string constants are an implicit contract with the live game page — if the game
/// changes its locale or notification wording, these values must be updated to match.
/// </summary>
public static class GameStatus
{
    private static StatusMessages Msg =>
        AppSettingsProvider.GameSettings.StatusMessages;

    public static IReadOnlyList<string> GameStartOrEnd => new[]
    {
        Msg.YourTurnFirst, Msg.YourTurn,
        Msg.OpponentFirst, Msg.OpponentTurn,
        Msg.YouWon, Msg.YouLost,
        Msg.OpponentLeft, Msg.ServerError, Msg.GameError
    };

    public static IReadOnlyList<string> TurnOrEnd => new[]
    {
        Msg.YourTurnFirst, Msg.YourTurn,
        Msg.YouWon, Msg.YouLost,
        Msg.OpponentLeft, Msg.ServerError, Msg.GameError
    };

    public static bool IsYourTurn(string status) =>
        status.Contains(Msg.YourTurn,      StringComparison.OrdinalIgnoreCase) ||
        status.Contains(Msg.YourTurnFirst, StringComparison.OrdinalIgnoreCase);

    public static bool IsOpponentTurn(string status) =>
        status.Contains(Msg.OpponentTurn,  StringComparison.OrdinalIgnoreCase) ||
        status.Contains(Msg.OpponentFirst, StringComparison.OrdinalIgnoreCase);

    public static bool IsVictory(string status) =>
        status.Contains(Msg.YouWon, StringComparison.OrdinalIgnoreCase);

    public static bool IsDefeat(string status) =>
        status.Contains(Msg.YouLost, StringComparison.OrdinalIgnoreCase);

    public static bool IsOpponentLeft(string status) =>
        status.Contains(Msg.OpponentLeft, StringComparison.OrdinalIgnoreCase);

    public static bool IsConnectionLost(string status) =>
        status.Contains(Msg.ServerError, StringComparison.OrdinalIgnoreCase) ||
        status.Contains(Msg.GameError,   StringComparison.OrdinalIgnoreCase);
}
