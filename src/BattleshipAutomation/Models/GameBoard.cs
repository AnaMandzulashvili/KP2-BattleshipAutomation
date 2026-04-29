using BattleshipAutomation.Enums;

namespace BattleshipAutomation.Models;

/// <summary>Represents the opponent's board with all known cell states and remaining fleet.</summary>
public class GameBoard
{
    public int Size { get; }

    private readonly Cell[,] _cells;
    private readonly Ship[]  _fleet;

    public IEnumerable<Ship> RemainingShips     => _fleet.Where(s => !s.IsSunk);
    public IEnumerable<int>  RemainingShipSizes => RemainingShips.Select(s => s.Size);

    public GameBoard(int size, IReadOnlyList<int> fleet)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Board size must be greater than zero.");
        if (fleet == null || fleet.Count == 0)
            throw new ArgumentException("Fleet must contain at least one ship.", nameof(fleet));
        if (fleet.Any(s => s > size))
            throw new ArgumentException($"No ship may be larger than the board size ({size}).", nameof(fleet));

        Size   = size;
        _cells = new Cell[size, size];

        for (var r = 0; r < size; r++)
            for (var c = 0; c < size; c++)
                _cells[r, c] = new Cell(r, c);

        _fleet = fleet.Select(s => new Ship(s)).ToArray();
    }

    public Cell GetCell(int row, int col) => _cells[row, col];

    public void SetCellState(int row, int col, CellState state) =>
        _cells[row, col].State = state;

    public IEnumerable<Cell> GetActiveHitCells() => GetCells(CellState.Hit);

    public bool AllShipsSunk() => _fleet.All(s => s.IsSunk);

    public bool IsInBounds(int row, int col) =>
        row >= 0 && row < Size && col >= 0 && col < Size;

    public void ApplySnapshot(CellState[,] snapshot)
    {
        for (var r = 0; r < Size; r++)
            for (var c = 0; c < Size; c++)
            {
                var newState = snapshot[r, c];
                if (newState == CellState.Unknown) continue;
                if (_cells[r, c].State == CellState.Sunk) continue; // never downgrade
                if (_cells[r, c].State != newState)
                    _cells[r, c].State = newState;
            }
    }

    public void MarkShipSunk(int shipSize)
    {
        var ship = RemainingShips.FirstOrDefault(s => s.Size == shipSize)
            ?? throw new InvalidOperationException(
                $"No remaining ship of size {shipSize} to mark as sunk.");
        ship.MarkSunk();
    }

    public void UnmarkShipSunk(int shipSize)
    {
        var ship = _fleet.LastOrDefault(s => s.IsSunk && s.Size == shipSize)
            ?? throw new InvalidOperationException(
                $"No sunk ship of size {shipSize} to unmark.");
        ship.UnmarkSunk();
    }

    private IEnumerable<Cell> GetCells(CellState state)
    {
        for (var r = 0; r < Size; r++)
            for (var c = 0; c < Size; c++)
                if (_cells[r, c].State == state)
                    yield return _cells[r, c];
    }
}
