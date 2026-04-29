using Aquality.Selenium.Browsers;
using BattleshipAutomation.Config;
using BattleshipAutomation.Utils.FrameworkAdditions;
using NLog;
using OpenQA.Selenium;
using Reqnroll;

namespace BattleshipAutomation.Utils.Hooks;

[Binding]
public sealed class Hooks
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly ScenarioContext _scenarioContext;
    private readonly GameSettings _settings;

    public Hooks(ScenarioContext scenarioContext, GameSettings settings)
    {
        _scenarioContext = scenarioContext;
        _settings = settings;
    }

    [BeforeTestRun(Order = 1)]
    public static void BeforeTestRun()
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.config");
        Reporter.InitialiseReports();
        Logger.Info("Test run started.");
    }

    [LogMethod]
    [BeforeScenario(Order = 1)]
    public void BeforeScenarioLogging()
    {
        Logger.Debug($"Starting scenario: {_scenarioContext.ScenarioInfo.Title}");
    }

    [LogMethod]
    [BeforeScenario(Order = 2)]
    public void BeforeScenarioNavigation()
    {
        try
        {
            AqualityServices.Browser.GoTo(_settings.BaseUrl);
        }
        catch (WebDriverException ex) when (ex is WebDriverTimeoutException || ex.Message.Contains("renderer", StringComparison.OrdinalIgnoreCase))
        {
            Logger.Warn("Renderer timeout on initial navigation; page may still be loading.");
        }
    }

    [LogMethod]
    [AfterScenario(Order = 1)]
    public void AfterScenario()
    {
        Logger.Debug($"Scenario '{_scenarioContext.ScenarioInfo.Title}' finished: {_scenarioContext.ScenarioExecutionStatus}");

        if (AqualityServices.IsBrowserStarted)
            AqualityServices.Browser.Quit();
    }

    [LogMethod]
    [AfterTestRun(Order = 1)]
    public static void AfterTestRun()
    {
        Reporter.Flush();
        Logger.Info("Test run finished.");
    }
}
