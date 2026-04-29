using Aquality.Selenium.Browsers;
using Aquality.Selenium.Elements.Interfaces;
using BattleshipAutomation.Config;
using BattleshipAutomation.Extensions.Selenium;
using BattleshipAutomation.Utils;
using OpenQA.Selenium;

namespace BattleshipAutomation.Screens;

public sealed class SetupScreen : BaseScreen, ISetupScreen
{
    private const string RandomOpponentClass = "battlefield-start-choose_rival-variant-link";

    private readonly TimeoutSettings _timeouts;

    public SetupScreen(TimeoutSettings timeouts)
        : base(By.TagName("body"), "Setup Screen")
    {
        _timeouts = timeouts;
    }

    private ILabel RandomOpponentLink => ElementFactory.GetLabel(
        By.XPath($"//a[contains(@class,'{RandomOpponentClass}')][normalize-space()='random']"),
        "Random Opponent");

    private ILabel RandomiseLabel => ElementFactory.GetLabel(
        By.CssSelector("li.placeships-variant__randomly .placeships-variant-link"),
        "Randomise");

    private IButton PlayButton => ElementFactory.GetButton(
        By.CssSelector(".battlefield-start-button"),
        "Play");

    public override bool IsPageContentDisplayed() => RandomiseLabel.State.IsExist;

    public void WaitForSetupScreen() =>
        WaitsHelper.WaitForTrue(
            IsPageContentDisplayed,
            _timeouts.PageLoad,
            "Setup screen did not load — Randomise button not visible.");

    public void ClickRandomise() => RandomiseLabel.JsClick();

    public void PlaceFleetRandomly(int minClicks, int maxClicks)
    {
        var clicks = Random.Shared.Next(minClicks, maxClicks + 1);
        for (var i = 0; i < clicks; i++)
            ClickRandomise();
    }

    public bool IsReadyToPlay() => PlayButton.State.IsExist;

    public void StartGameWithRandomOpponent()
    {
        // Neutralise the href so clicking the <a> tag does not trigger a page navigation —
        // Chrome 147 hangs waiting for renderer ACK whenever an <a> element navigates.
        AqualityServices.Browser.ExecuteScript(
            $"document.querySelector(\"a.{RandomOpponentClass}\").href='javascript:void(0)'");
        RandomOpponentLink.JsClick();
        PlayButton.JsClick();
    }
}
