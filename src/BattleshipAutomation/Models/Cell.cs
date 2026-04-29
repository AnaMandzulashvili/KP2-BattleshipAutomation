using BattleshipAutomation.Enums;

namespace BattleshipAutomation.Models;

/// <summary>Represents a single cell on the opponent's Battleship grid.</summary>
public class Cell
{
    public int       Row   { get; }
    public int       Col   { get; }
    public CellState State { get; internal set; } = CellState.Unknown;

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public override string ToString() => $"({Row},{Col}):{State}";
}
