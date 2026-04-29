using BattleshipAutomation.Enums;

namespace BattleshipAutomation.Game;

public interface IGameService
{
    string      WaitForOpponent();
    GameOutcome PlayUntilEnd();
}
