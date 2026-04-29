using MethodDecorator.Fody.Interfaces;
using NLog;
using System.Reflection;

namespace BattleshipAutomation.Utils.FrameworkAdditions;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class LogMethodAttribute : Attribute, IMethodDecorator
{
    private ILogger? _logger;
    private string?  _methodName;

    public void Init(object instance, MethodBase method, object[] args)
    {
        _logger     = LogManager.GetLogger(method.DeclaringType?.FullName ?? "Unknown");
        _methodName = method.Name;
    }

    public void OnEntry()   => _logger!.Debug($"→ {_methodName}");
    public void OnExit()    => _logger!.Debug($"← {_methodName}");

    public void OnException(Exception ex) =>
        _logger!.Error($"✗ {_methodName}: {ex.GetType().Name}: {ex.Message}");
}
