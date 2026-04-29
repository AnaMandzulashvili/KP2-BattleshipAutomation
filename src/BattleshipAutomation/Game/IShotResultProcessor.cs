using BattleshipAutomation.Enums;
using BattleshipAutomation.Models;

namespace BattleshipAutomation.Game;

public interface IShotResultProcessor
{
    (bool isSunk, int sunkSize) ProcessSnapshot(
        GameBoard    board,
        CellState[,] snapshot,
        int          firedRow,
        int          firedCol);
}
