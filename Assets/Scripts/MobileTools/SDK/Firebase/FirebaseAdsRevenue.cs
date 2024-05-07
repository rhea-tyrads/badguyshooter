//using Firebase.Analytics;

using UnityEngine;

namespace MobileTools.SDK.Firebase
{
    public static class Events
    {
        public const string LEVEL_COMPLETE = "LevelComplete";
        public const string LEVEL_COMPLETE_DURATION = "LevelCompleteTime";
        public const string FAILS_PER_LEVEL = "FailsPerLevel";
        public const string DAILY_PLAYTIME = "DailyPlayTime";
        public const string DAY_3_PLAYTIME = "Day3PlayTime";
        public const string FIRST_SESSION_DURATION = "FirstSessionDuration";
        public const string EXIT_THE_GAME = "ExitTheGame";
    }

    public static class Params
    {
        public const string NUMBER = "Number";
        public const string SECONDS = "Seconds";
        public const string FAILS = "LevelFails";
    }

    public class FirebaseAdsRevenue : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);

            //MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            //MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            //MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            //MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }

        //void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
        //{
        //    Debug.LogError("[Firebase] Ad revenue send");

        //    double revenue = impressionData.Revenue;
        //    var impressionParameters = new[]
        //    {
        //        new Parameter("ad_platform", "AppLovin"),
        //        new Parameter("ad_source", impressionData.NetworkName),
        //        new Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
        //        new Parameter("ad_format", impressionData.AdFormat),
        //        new Parameter("value", revenue),
        //        new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        //    };

        //    FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        //}
    }
}