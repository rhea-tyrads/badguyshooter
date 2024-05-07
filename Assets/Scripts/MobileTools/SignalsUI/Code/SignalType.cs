namespace MobileTools.SignalsUI.Code
{
    [System.Flags]
    public enum SignalType
    {
        None = 0,
        TodayLevel = 1 << 0,
        HeroUpgrade = 1 << 1,
        Leaderboard = 1 << 2,
        LuckyWheel = 1 << 3,
        DayilyLogin = 1 << 4


    }
}
