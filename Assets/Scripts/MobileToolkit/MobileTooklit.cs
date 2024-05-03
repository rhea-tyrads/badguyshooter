using CoreUI;
using UnityEngine;

public class MobileTooklit : MonoBehaviour
{
    public MobileTooklitSettings settings;

    [Space(20)]
    public EulaPrivacy eula;
    public VersionCheck versionCheck;
    public InternetConnection internetConnection;
    public RateGame rateGame;
    public DailyLogin dailyLogin;
    public Leaderboard leaderboard;


    void Start()
    {
        Init();
    }

    void Init()
    {
        eula.Init(settings);
        versionCheck.Init(settings);
        internetConnection.Init(settings);
        rateGame.Init(settings);
        dailyLogin.Init(settings);
        leaderboard.Init(settings);
    }
}
