using BattleshipAutomation.Config;
using BattleshipAutomation.Game;
using BattleshipAutomation.Models;
using BattleshipAutomation.Screens;
using Reqnroll;
using Reqnroll.BoDi;

namespace BattleshipAutomation.Utils.Hooks;

/// <summary>Composition root — wires up and registers all per-scenario dependencies.</summary>
[Binding]
public static class ScenarioDependencies
{
    [BeforeScenario(Order = 0)]
    public static void Register(IObjectContainer container)
    {
        var settings = AppSettingsProvider.GameSettings;
        var timeouts = settings.Timeouts;
        var board = new GameBoard(settings.BoardSize, settings.Fleet);
        var setupScreen = new SetupScreen(timeouts);
        var gameScreen = new GameScreen(settings.BoardSize);
        var processor = new ShotResultProcessor();
        var strategy = new HuntAndTargetStrategy(board);
        var service = new GameService(gameScreen, board, strategy, processor, timeouts);

        container.RegisterInstanceAs(settings);
        container.RegisterInstanceAs(timeouts);
        container.RegisterInstanceAs<ISetupScreen>(setupScreen);
        container.RegisterInstanceAs<IGameScreen>(gameScreen);
        container.RegisterInstanceAs<IGameService>(service);
    }
}
