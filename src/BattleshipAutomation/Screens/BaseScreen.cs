using Aquality.Selenium.Forms;
using OpenQA.Selenium;

namespace BattleshipAutomation.Screens;

public abstract class BaseScreen : Form
{
    protected BaseScreen(By locator, string name) : base(locator, name)
    {
    }

    public abstract bool IsPageContentDisplayed();
}
