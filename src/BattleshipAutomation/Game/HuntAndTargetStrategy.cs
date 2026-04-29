using BattleshipAutomation.Enums;
using BattleshipAutomation.Models;
using NLog;

namespace BattleshipAutomation.Game;

/// <summary>
/// Hunt &amp; Target strategy with Probability Density overlay.
///
/// Hunt phase: fires at the highest-probability unknown cell using a full density map
/// (counts all valid ship placements per cell) with checkerboard parity bias.
///
/// Target phase: after a hit, queues adjacent cells; locks axis on the second hit;
/// extends along that axis until the ship sinks.
/// </summary>
public class HuntAndTargetStrategy : IMoveStrategy
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private static readonly (int dr, int dc)[] CardinalDeltas =
        { (-1, 0), (1, 0), (0, -1), (0, 1) };

    private const int DensityWeight = 2;
    private const int ParityBonus   = 1;

    private readonly GameBoard              _board;
    private readonly Queue<Cell>            _targetQueue  = new();
    private readonly HashSet<(int, int)>    _queuedCoords = new();

    private Cell?         _firstHitInRun   = null;
    private HuntDirection _lockedDirection = HuntDirection.None;

    public HuntAndTargetStrategy(GameBoard board)
    {
        _board = board;
    }

    public Cell SelectNextTarget()
    {
        if (TryDequeueUnknown(out var target))
            return target!;

        var activeHits = _board.GetActiveHitCells().ToList();
        if (activeHits.Count > 0)
        {
            _firstHitInRun   = activeHits[0];
            _lockedDirection = HuntDirection.None;
            foreach (var hit in activeHits)
                EnqueueAdjacent(hit, HuntDirection.None);

            if (TryDequeueUnknown(out target))
                return target!;
        }

        _firstHitInRun   = null;
        _lockedDirection = HuntDirection.None;
        return SelectHuntCell();
    }

    private bool TryDequeueUnknown(out Cell? cell)
    {
        while (_targetQueue.Count > 0)
        {
            var candidate = _targetQueue.Dequeue();
            _queuedCoords.Remove((candidate.Row, candidate.Col));

            if (_board.GetCell(candidate.Row, candidate.Col).State == CellState.Unknown)
            {
                cell = candidate;
                return true;
            }
        }

        cell = null;
        return false;
    }

    public void RecordShotResult(Cell cell, bool isHit, bool isSunk)
    {
        if (!isHit)
            return;

        if (isSunk)
        {
            HandleSunk();
            return;
        }

        if (_firstHitInRun == null)
        {
            _firstHitInRun   = cell;
            _lockedDirection = HuntDirection.None;
            EnqueueAdjacent(cell, HuntDirection.None);
            return;
        }

        if (_lockedDirection == HuntDirection.None)
        {
            _lockedDirection = cell.Row == _firstHitInRun.Row
                ? HuntDirection.Horizontal
                : HuntDirection.Vertical;
            RebuildDirectedQueue();
        }
        else
        {
            EnqueueExtension(cell);
        }
    }

    private Cell SelectHuntCell()
    {
        var densityMap = BuildDensityMap();
        Cell? best     = null;
        var bestScore  = -1;

        for (var r = 0; r < _board.Size; r++)
        {
            for (var c = 0; c < _board.Size; c++)
            {
                if (_board.GetCell(r, c).State != CellState.Unknown)
                    continue;

                var parityBonus    = (r + c) % 2 == 0 ? ParityBonus : 0;
                var effectiveScore = densityMap[r, c] * DensityWeight + parityBonus;

                if (effectiveScore > bestScore)
                {
                    bestScore = effectiveScore;
                    best      = _board.GetCell(r, c);
                }
            }
        }

        return best ?? throw new InvalidOperationException("No valid cells remain on the board.");
    }

    private int[,] BuildDensityMap()
    {
        var map = new int[_board.Size, _board.Size];

        foreach (var shipSize in _board.RemainingShipSizes)
        {
            for (var r = 0; r < _board.Size; r++)
                for (var c = 0; c <= _board.Size - shipSize; c++)
                    if (PlacementIsValid(r, c, 0, 1, shipSize))
                        for (var k = 0; k < shipSize; k++)
                            map[r, c + k]++;

            for (var r = 0; r <= _board.Size - shipSize; r++)
                for (var c = 0; c < _board.Size; c++)
                    if (PlacementIsValid(r, c, 1, 0, shipSize))
                        for (var k = 0; k < shipSize; k++)
                            map[r + k, c]++;
        }

        return map;
    }

    private bool PlacementIsValid(int startRow, int startCol, int dr, int dc, int size)
    {
        for (var k = 0; k < size; k++)
        {
            var state = _board.GetCell(startRow + dr * k, startCol + dc * k).State;
            if (state == CellState.Miss || state == CellState.Sunk)
                return false;
        }
        return true;
    }

    private void EnqueueAdjacent(Cell origin, HuntDirection direction)
    {
        foreach (var (dr, dc) in CardinalDeltas)
        {
            if (direction == HuntDirection.Horizontal && dr != 0) continue;
            if (direction == HuntDirection.Vertical   && dc != 0) continue;

            TryEnqueue(origin.Row + dr, origin.Col + dc);
        }
    }

    private void EnqueueExtension(Cell latestHit)
    {
        var dr = _lockedDirection == HuntDirection.Vertical   ? 1 : 0;
        var dc = _lockedDirection == HuntDirection.Horizontal ? 1 : 0;

        TryEnqueue(latestHit.Row + dr, latestHit.Col + dc);
        TryEnqueue(latestHit.Row - dr, latestHit.Col - dc);
    }

    private void RebuildDirectedQueue()
    {
        _targetQueue.Clear();
        _queuedCoords.Clear();
        foreach (var hit in _board.GetActiveHitCells())
            EnqueueAdjacent(hit, _lockedDirection);
    }

    private void TryEnqueue(int r, int c)
    {
        if (_board.IsInBounds(r, c)
            && _board.GetCell(r, c).State == CellState.Unknown
            && _queuedCoords.Add((r, c)))
        {
            _targetQueue.Enqueue(_board.GetCell(r, c));
        }
    }

    private void HandleSunk()
    {
        _targetQueue.Clear();
        _queuedCoords.Clear();
        _firstHitInRun   = null;
        _lockedDirection = HuntDirection.None;
    }
}
