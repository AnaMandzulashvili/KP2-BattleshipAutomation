using BattleshipAutomation.Models;

namespace BattleshipAutomation.Game;

public interface IMoveStrategy
{
    Cell SelectNextTarget();
    void RecordShotResult(Cell cell, bool isHit, bool isSunk);
}
