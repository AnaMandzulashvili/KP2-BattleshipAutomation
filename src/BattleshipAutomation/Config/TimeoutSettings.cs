namespace BattleshipAutomation.Config;

public class TimeoutSettings
{
    public int OpponentWaitSeconds { get; init; } = 180;
    public int TurnWaitSeconds     { get; init; } = 120;
    public int ShotResultSeconds   { get; init; } = 15;
    public int PageLoadSeconds     { get; init; } = 60;
    public int ShotSettleSeconds   { get; init; } = 3;

    public TimeSpan OpponentWait => TimeSpan.FromSeconds(OpponentWaitSeconds);
    public TimeSpan TurnWait     => TimeSpan.FromSeconds(TurnWaitSeconds);
    public TimeSpan ShotResult   => TimeSpan.FromSeconds(ShotResultSeconds);
    public TimeSpan PageLoad     => TimeSpan.FromSeconds(PageLoadSeconds);
    public TimeSpan ShotSettle   => TimeSpan.FromSeconds(ShotSettleSeconds);
}
