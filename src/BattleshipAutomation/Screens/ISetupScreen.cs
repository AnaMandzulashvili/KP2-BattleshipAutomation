namespace BattleshipAutomation.Screens;

public interface ISetupScreen
{
    void WaitForSetupScreen();
    void PlaceFleetRandomly(int minClicks, int maxClicks);
    bool IsReadyToPlay();
    void StartGameWithRandomOpponent();
}
