namespace BattleshipAutomation.Config;

/// <summary>UI notification texts matched against the live game page.</summary>
public class StatusMessages
{
    public string YourTurnFirst  { get; init; } = "The game started, your turn.";
    public string YourTurn       { get; init; } = "Your turn.";
    public string OpponentFirst  { get; init; } = "The game began, opponent's turn.";
    public string OpponentTurn   { get; init; } = "Opponent's turn, please wait.";
    public string YouWon         { get; init; } = "Game over. Congratulations, you won!";
    public string YouLost        { get; init; } = "Game over. You lose.";
    public string OpponentLeft   { get; init; } = "Your opponent has left the game.";
    public string ServerError    { get; init; } = "Server is unavailable.";
    public string GameError      { get; init; } = "Unexpected error. Further play is impossible.";
}
