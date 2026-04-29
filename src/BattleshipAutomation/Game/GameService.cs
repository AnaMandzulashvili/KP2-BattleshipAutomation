using BattleshipAutomation.Config;
using BattleshipAutomation.Enums;
using BattleshipAutomation.Models;
using BattleshipAutomation.Screens;
using BattleshipAutomation.Utils;
using NLog;

namespace BattleshipAutomation.Game;

/// <summary>Orchestrates the game loop: waits for turns, fires shots, and returns the final outcome.</summary>
public class GameService : IGameService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly IGameScreen _screen;
    private readonly GameBoard _board;
    private readonly IMoveStrategy _strategy;
    private readonly IShotResultProcessor _processor;
    private readonly TimeoutSettings _timeouts;

    public GameService(IGameScreen screen, GameBoard board, IMoveStrategy strategy, IShotResultProcessor processor, TimeoutSettings timeouts)
    {
        _screen = screen;
        _board = board;
        _strategy = strategy;
        _processor = processor;
        _timeouts = timeouts;
    }

    public string WaitForOpponent() =>
        WaitForStatus(GameStatus.GameStartOrEnd, _timeouts.OpponentWait);

    public GameOutcome PlayUntilEnd()
    {
        while (true)
        {
            var status = WaitForTurnOrEnd();

            if (!GameStatus.IsYourTurn(status))
                return ClassifyOutcome(status);

            ExecuteTurn();
        }
    }

    private void ExecuteTurn()
    {
        var target = _strategy.SelectNextTarget();
        Logger.Debug($"Firing at ({target.Row},{target.Col}).");
        _screen.FireAtCell(target.Row, target.Col);

        var (cellState, snapshot) = WaitForShotResult(target.Row, target.Col);
        var (isSunk, sunkSize) = _processor.ProcessSnapshot(_board, snapshot, target.Row, target.Col);

        var effectiveState = _board.GetCell(target.Row, target.Col).State;
        if (effectiveState == CellState.Unknown)
            effectiveState = cellState;

        if (isSunk)
            Logger.Debug($"Ship of size {sunkSize} sunk.");

        _strategy.RecordShotResult(
            target,
            isHit: effectiveState != CellState.Miss,
            isSunk: isSunk);
    }

    private string WaitForTurnOrEnd() =>
        WaitForStatus(GameStatus.TurnOrEnd, _timeouts.TurnWait);

    private (CellState state, CellState[,] snapshot) WaitForShotResult(int row, int col)
    {
        var state = CellState.Unknown;

        WaitsHelper.WaitForTrue(
            () =>
            {
                state = _screen.GetCellState(row, col);
                return state != CellState.Unknown;
            },
            _timeouts.ShotResult,
            $"Shot result not received for cell ({row},{col}) within {_timeouts.ShotResult.TotalSeconds}s.");

        if (state != CellState.Miss)
        {
            WaitsHelper.WaitFor(
                () => _screen.GetCellState(row, col) == CellState.Sunk,
                _timeouts.ShotSettle);

            if (_screen.GetCellState(row, col) == CellState.Sunk)
            {
                var snapshot = WaitForSnapshotStable();
                return (CellState.Sunk, snapshot);
            }
        }

        return (_screen.GetCellState(row, col), _screen.GetBoardSnapshot());
    }

    private CellState[,] WaitForSnapshotStable()
    {
        var prevCount = -1;
        CellState[,] snap = null!;

        WaitsHelper.WaitFor(
            () =>
            {
                snap = _screen.GetBoardSnapshot();
                var count = CountSunk(snap);
                Logger.Debug($"Snapshot stabilisation: sunk={count}");
                if (count == prevCount) return true;
                prevCount = count;
                return false;
            },
            _timeouts.ShotSettle);

        return snap;
    }

    private static int CountSunk(CellState[,] snap)
    {
        var count = 0;
        for (var r = 0; r < snap.GetLength(0); r++)
            for (var c = 0; c < snap.GetLength(1); c++)
                if (snap[r, c] == CellState.Sunk)
                    count++;
        return count;
    }

    private string WaitForStatus(IReadOnlyList<string> expectedTexts, TimeSpan timeout)
    {
        var matched = string.Empty;
        WaitsHelper.WaitForTrue(
            () =>
            {
                var current = _screen.GetStatusText();
                matched = expectedTexts.FirstOrDefault(e =>
                    current.Contains(e, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                return matched != string.Empty;
            },
            timeout,
            $"Expected one of: [{string.Join(", ", expectedTexts)}]");
        return matched;
    }

    private static GameOutcome ClassifyOutcome(string status)
    {
        if (GameStatus.IsVictory(status))
        {
            Logger.Info("Victory — opponent's fleet destroyed.");
            return GameOutcome.Victory;
        }
        if (GameStatus.IsDefeat(status)) return GameOutcome.Defeat;
        if (GameStatus.IsOpponentLeft(status)) return GameOutcome.OpponentLeft;
        if (GameStatus.IsConnectionLost(status)) return GameOutcome.ConnectionLost;

        throw new InvalidOperationException($"Unrecognized game status: '{status}'");
    }
}
