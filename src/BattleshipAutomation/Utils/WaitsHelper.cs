using Aquality.Selenium.Browsers;

namespace BattleshipAutomation.Utils;

public static class WaitsHelper
{
    public static void WaitForTrue(Func<bool> condition, TimeSpan timeout, string message = "") =>
        AqualityServices.ConditionalWait.WaitForTrue(condition, timeout, message: message);

    public static bool WaitFor(Func<bool> condition, TimeSpan timeout) =>
        AqualityServices.ConditionalWait.WaitFor(condition, timeout);
}
