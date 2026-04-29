using BattleshipAutomation.Config;
using BattleshipAutomation.Enums;
using BattleshipAutomation.Game;
using BattleshipAutomation.Screens;
using NUnit.Framework;
using Reqnroll;

namespace BattleshipAutomation.Steps;

[Binding]
public class BattleshipGameSteps
{
    private readonly ISetupScreen _setupScreen;
    private readonly IGameService _service;
    private readonly GameSettings _settings;

    private GameOutcome _outcome = GameOutcome.Unknown;

    public BattleshipGameSteps(ISetupScreen setupScreen, IGameService service, GameSettings settings)
    {
        _setupScreen = setupScreen;
        _service = service;
        _settings = settings;
    }

    [Given(@"The Battleship game page is loaded")]
    public void GivenTheBattleshipGamePageIsLoaded()
    {
        _setupScreen.WaitForSetupScreen();
    }

    [When(@"My fleet is placed randomly")]
    public void WhenMyFleetIsPlacedRandomly()
    {
        _setupScreen.PlaceFleetRandomly(
            _settings.MinRandomiseClicks,
            _settings.MaxRandomiseClicks);
    }

    [Then(@"The board is ready to play")]
    public void ThenTheBoardIsReadyToPlay()
    {
        Assert.That(_setupScreen.IsReadyToPlay(), Is.True,
            "Play button is not visible — board is not ready.");
    }

    [When(@"I start a game against a random opponent")]
    public void WhenIStartAGameAgainstARandomOpponent()
    {
        _setupScreen.StartGameWithRandomOpponent();
        _service.WaitForOpponent();
    }

    [When(@"I play until the game ends")]
    public void WhenIPlayUntilTheGameEnds()
    {
        _outcome = _service.PlayUntilEnd();
    }

    [Then(@"I win the game")]
    public void ThenIWinTheGame()
    {
        Assert.That(_outcome, Is.EqualTo(GameOutcome.Victory),
            $"Expected Victory but got: {_outcome}.");
    }
}
