using Aquality.Selenium.Browsers;
using AventStack.ExtentReports;
using BattleshipAutomation.Utils.FrameworkAdditions;
using NLog;
using OpenQA.Selenium;
using Reqnroll;

namespace BattleshipAutomation.Utils.Hooks;

[Binding]
public static class ReportHooks
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    [LogMethod]
    [BeforeFeature]
    public static void BeforeFeature(FeatureContext featureContext)
    {
        Reporter.CreateFeature(featureContext.FeatureInfo.Title);
    }

    [LogMethod]
    [BeforeScenario(Order = 3)]
    public static void BeforeScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
    {
        Reporter.CreateScenario(featureContext.FeatureInfo.Title, scenarioContext.ScenarioInfo.Title);
    }

    [LogMethod]
    [AfterStep]
    public static void AfterStep(ScenarioContext scenarioContext)
    {
        var stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
        var stepText = scenarioContext.StepContext.StepInfo.Text;

        if (scenarioContext.TestError == null)
        {
            Reporter.LogStep(Status.Pass, $"{stepType} {stepText}");
        }
        else
        {
            TryCaptureScreenshot();
            Reporter.LogStep(Status.Fail, $"{stepType} {stepText}<br>{scenarioContext.TestError.Message}");
        }
    }

    private static void TryCaptureScreenshot()
    {
        if (!AqualityServices.IsBrowserStarted)
            return;

        try
        {
            var base64 = ((ITakesScreenshot)AqualityServices.Browser.Driver)
                .GetScreenshot().AsBase64EncodedString;
            Reporter.AttachScreenshot(base64);
        }
        catch (InvalidCastException ex)
        {
            Logger.Warn($"Screenshot capture failed — driver does not support ITakesScreenshot: {ex.Message}");
        }
        catch (WebDriverException ex)
        {
            Logger.Warn($"Screenshot capture failed: {ex.GetType().Name}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Logger.Warn($"Screenshot capture failed: {ex.Message}");
        }
    }
}
