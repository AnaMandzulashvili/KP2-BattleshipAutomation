using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace BattleshipAutomation.Utils;

public static class Reporter
{
    private static ExtentReports? _extent;
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ExtentTest> Features = new();
    private static readonly System.Threading.AsyncLocal<ExtentTest?> CurrentScenario = new();

    public static void InitialiseReports()
    {
        var reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "index.html");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

        var htmlReporter = new ExtentSparkReporter(reportPath);
        htmlReporter.Config.DocumentTitle = "Battleship Automation Report";
        htmlReporter.Config.ReportName    = "Battleship Game E2E Tests";
        htmlReporter.Config.Theme         = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

        _extent = new ExtentReports();
        _extent.AttachReporter(htmlReporter);
    }

    public static void CreateFeature(string name)
    {
        EnsureInitialised();
        Features.GetOrAdd(name, n => _extent!.CreateTest(n));
    }

    public static void CreateScenario(string featureName, string scenarioName)
    {
        EnsureInitialised();
        var feature = Features.GetOrAdd(featureName, n => _extent!.CreateTest(n));
        CurrentScenario.Value = feature.CreateNode(scenarioName);
    }

    public static void LogStep(Status status, string message) =>
        CurrentScenario.Value?.Log(status, message);

    public static void AttachScreenshot(string base64) =>
        CurrentScenario.Value?.AddScreenCaptureFromBase64String(base64, "Failure Screenshot");

    public static void Flush() => _extent?.Flush();

    private static void EnsureInitialised()
    {
        if (_extent == null)
            throw new InvalidOperationException(
                "Reporter.InitialiseReports() must be called before using the reporter.");
    }
}
