using Aquality.Selenium.Browsers;
using Aquality.Selenium.Elements.Interfaces;
using BattleshipAutomation.Enums;
using BattleshipAutomation.Extensions.Selenium;
using NLog;
using OpenQA.Selenium;

namespace BattleshipAutomation.Screens;

public sealed class GameScreen : BaseScreen, IGameScreen
{
    private new static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private const string RivalCellSelector =
        "div.battlefield.battlefield__rival .battlefield-cell-content";

    private const string DoneClass = "__done";
    private const string HitClass = "__hit";
    private const string MissClass = "__miss";

    private readonly int _boardSize;

    public GameScreen(int boardSize)
        : base(By.TagName("body"), "Game Screen")
    {
        _boardSize = boardSize;
    }

    private ILabel ActiveNotification => ElementFactory.GetLabel(
        By.CssSelector(".notification:not(.none) .notification-message"),
        "Active Notification");

    private IButton RivalCell(int row, int col) => ElementFactory.GetButton(
        By.CssSelector($"{RivalCellSelector}[data-y='{row}'][data-x='{col}']"),
        $"Cell ({row},{col})");

    public override bool IsPageContentDisplayed() =>
        ActiveNotification.State.IsExist;

    public string GetStatusText() =>
        ActiveNotification.State.IsExist
            ? ActiveNotification.Text.Trim()
            : string.Empty;

    public CellState[,] GetBoardSnapshot()
    {
        const string script =
            @"var size = arguments[0], sel = arguments[1];
              var out = [];
              for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                  var el = document.querySelector(sel + '[data-y=""' + y + '""][data-x=""' + x + '""]');
                  out.push(el ? ((el.closest('td') || {}).className || '') : '');
                }
              }
              return out;";

        var flat = AqualityServices.Browser.ExecuteScript<IList<object>>(
                       script, _boardSize, RivalCellSelector)
                   ?? new List<object>();

        var grid = new CellState[_boardSize, _boardSize];
        for (var i = 0; i < Math.Min(flat.Count, _boardSize * _boardSize); i++)
            grid[i / _boardSize, i % _boardSize] = ClassesToCellState(flat[i]?.ToString() ?? string.Empty);
        return grid;
    }

    public CellState GetCellState(int row, int col)
    {
        const string script =
            "var el = document.querySelector(arguments[0] + '[data-y=\"' + arguments[1] + '\"][data-x=\"' + arguments[2] + '\"]');" +
            "if (!el) return '';" +
            "return (el.closest('td') || {}).className || '';";

        var classes = AqualityServices.Browser.ExecuteScript<string>(
                          script, RivalCellSelector, row, col)
                      ?? string.Empty;

        return ClassesToCellState(classes);
    }

    private static CellState ClassesToCellState(string classes)
    {
        if (string.IsNullOrEmpty(classes))
        {
            Logger.Warn("Cell element or parent <td> not found in DOM — returning Unknown.");
            return CellState.Unknown;
        }
        if (classes.Contains(DoneClass, StringComparison.OrdinalIgnoreCase)) return CellState.Sunk;
        if (classes.Contains(HitClass, StringComparison.OrdinalIgnoreCase)) return CellState.Hit;
        if (classes.Contains(MissClass, StringComparison.OrdinalIgnoreCase)) return CellState.Miss;
        return CellState.Unknown;
    }

    public void FireAtCell(int row, int col) => RivalCell(row, col).JsClick();
}
