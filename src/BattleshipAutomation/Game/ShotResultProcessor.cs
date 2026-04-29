using BattleshipAutomation.Enums;
using BattleshipAutomation.Models;
using NLog;

namespace BattleshipAutomation.Game;

/// <summary>
/// Detects sunk ships after each shot by scanning for newly-Sunk cells in the snapshot.
///
/// Split __done handling: when the DOM delivers __done (Sunk) for a ship's cells across
/// multiple snapshots, we defer marking until the component is "enclosed" — all adjacent
/// cells are Miss, OOB, or already-Sunk. This guarantees we only mark a ship when we
/// have seen all of its cells. If a partially-sunk component later grows, we unmark the
/// previous size and re-mark with the full component.
/// </summary>
public class ShotResultProcessor : IShotResultProcessor
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private static readonly (int dr, int dc)[] CardinalDeltas =
        { (-1, 0), (1, 0), (0, -1), (0, 1) };

    private readonly List<(HashSet<(int, int)> cells, int size)> _markedShips = new();

    public (bool isSunk, int sunkSize) ProcessSnapshot(
        GameBoard board,
        CellState[,] snapshot,
        int firedRow,
        int firedCol)
    {
        var alreadySunk = CollectSunkPositions(board);
        board.ApplySnapshot(snapshot);

        var visited = new HashSet<(int, int)>();
        var primarySunk = false;
        var primarySunkSize = 0;

        for (var r = 0; r < board.Size; r++)
        {
            for (var c = 0; c < board.Size; c++)
            {
                if (board.GetCell(r, c).State != CellState.Sunk) continue;
                if (visited.Contains((r, c))) continue;

                var component = FloodFill(board, r, c, visited);
                foreach (var pos in component) visited.Add(pos);

                var newCells = component.Where(p => !alreadySunk.Contains(p)).ToHashSet();
                if (newCells.Count == 0)
                    continue;

                var overlapping = _markedShips.Where(e => e.cells.Overlaps(component)).ToList();
                foreach (var prev in overlapping)
                {
                    Logger.Debug($"Split __done: unmarking size={prev.size}, new component size={component.Count}.");
                    board.UnmarkShipSunk(prev.size);
                    _markedShips.Remove(prev);
                }

                var shipSize = ResolveShipSize(board, component, firedRow, firedCol);

                if (shipSize == 0)
                {
                    Logger.Debug($"Partial __done at ({r},{c}), component size={component.Count} — deferring.");
                    continue;
                }

                Logger.Debug($"Sunk component size={component.Count} resolved={shipSize} at ({r},{c}). Remaining: [{string.Join(",", board.RemainingShipSizes)}]");
                board.MarkShipSunk(shipSize);
                _markedShips.Add((component, shipSize));

                if (component.Contains((firedRow, firedCol)))
                {
                    primarySunk = true;
                    primarySunkSize = shipSize;
                }
            }
        }

        return (primarySunk, primarySunkSize);
    }

    private static HashSet<(int, int)> FloodFill(
        GameBoard board, int startR, int startC, HashSet<(int, int)> visited)
    {
        var component = new HashSet<(int, int)>();
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((startR, startC));

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            if (!board.IsInBounds(r, c)) continue;
            if (visited.Contains((r, c)) || component.Contains((r, c))) continue;
            if (board.GetCell(r, c).State != CellState.Sunk) continue;

            component.Add((r, c));
            foreach (var (dr, dc) in CardinalDeltas)
                queue.Enqueue((r + dr, c + dc));
        }

        return component;
    }

    private static int ResolveShipSize(
        GameBoard board,
        HashSet<(int, int)> component,
        int firedRow, int firedCol)
    {
        var remaining = board.RemainingShipSizes.ToList();
        if (!remaining.Any()) return 0;

        if (remaining.Contains(component.Count))
            return component.Count;

        if (component.Contains((firedRow, firedCol)))
        {
            foreach (var candidate in StraightLineLengths(component, firedRow, firedCol))
                if (remaining.Contains(candidate))
                    return candidate;
        }

        if (!IsComponentEnclosed(board, component))
            return 0;

        var atLeast = remaining.Where(s => s >= component.Count).ToList();
        if (atLeast.Count > 0) return atLeast.Min();

        throw new InvalidOperationException(
            $"No remaining ship fits component of size {component.Count}. " +
            $"Remaining: [{string.Join(",", remaining)}]. Possible DOM/model desync.");
    }

    private static bool IsComponentEnclosed(GameBoard board, HashSet<(int, int)> component)
    {
        foreach (var (r, c) in component)
            foreach (var (dr, dc) in CardinalDeltas)
            {
                var nr = r + dr;
                var nc = c + dc;
                if (!board.IsInBounds(nr, nc)) continue;
                var state = board.GetCell(nr, nc).State;
                if (state is CellState.Unknown or CellState.Hit)
                    return false;
            }
        return true;
    }

    private static IEnumerable<int> StraightLineLengths(
        HashSet<(int, int)> cells, int row, int col)
    {
        yield return CountLine(cells, row, col, 0, 1) + CountLine(cells, row, col, 0, -1) - 1;
        yield return CountLine(cells, row, col, 1, 0) + CountLine(cells, row, col, -1, 0) - 1;
    }

    private static int CountLine(
        HashSet<(int, int)> cells, int r, int c, int dr, int dc)
    {
        var count = 0;
        while (cells.Contains((r, c))) { count++; r += dr; c += dc; }
        return count;
    }

    private static HashSet<(int, int)> CollectSunkPositions(GameBoard board)
    {
        var set = new HashSet<(int, int)>();
        for (var r = 0; r < board.Size; r++)
            for (var c = 0; c < board.Size; c++)
                if (board.GetCell(r, c).State == CellState.Sunk)
                    set.Add((r, c));
        return set;
    }
}
