using Aquality.Selenium.Elements.Interfaces;

namespace BattleshipAutomation.Extensions.Selenium;

public static class ElementExtensions
{
    public static void JsClick(this IElement element) => element.JsActions.Click();
}
