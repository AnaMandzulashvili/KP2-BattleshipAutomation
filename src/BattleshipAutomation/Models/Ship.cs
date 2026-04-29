namespace BattleshipAutomation.Models;

/// <summary>Represents a ship being tracked on the opponent's board.</summary>
public class Ship
{
    public int Size { get; }

    public bool IsSunk { get; private set; }

    public Ship(int size)
    {
        Size = size;
    }

    public void MarkSunk()   => IsSunk = true;
    public void UnmarkSunk() => IsSunk = false;
}
