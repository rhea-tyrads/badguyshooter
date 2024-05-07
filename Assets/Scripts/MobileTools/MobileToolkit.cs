using MobileTools.DailyLogins.Code;
using MobileTools.GameVersionCheck;
using MobileTools.InternetCheck;
using MobileTools.Leaderboards.Code;
using MobileTools.RateGameUI.Code;
using UnityEngine;

namespace MobileTools
{
    public class MobileToolkit : MonoBehaviour
    {
        public MobileTooklitSettings settings;

        [Space(20)]
        public EulaPrivacy.EulaPrivacy eula;
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
}
