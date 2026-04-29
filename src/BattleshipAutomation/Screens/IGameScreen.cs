using BattleshipAutomation.Enums;

namespace BattleshipAutomation.Screens;

public interface IGameScreen
{
    string       GetStatusText();
    CellState[,] GetBoardSnapshot();
    CellState    GetCellState(int row, int col);
    void         FireAtCell(int row, int col);
}
