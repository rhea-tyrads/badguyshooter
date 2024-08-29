using System;
using com.adjust.sdk;
using MobileTools.MonoCache.System;
using MobileTools.Utilities;
using UnityEngine;
using Utilities;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Applovin
{
    public class ApplovinController : Singleton<ApplovinController>
    {
        //  public SetupSO config;

        [Header("ID")]
        public string interstitialID = "8925df940997a3f3";
        public string rewardedID = "055c46acd5d7896b";

        [Space(20)]
        public Rewarded rewarded;
        public Interstitial interstitial;

        public event Action OnRewardReceived = delegate { };
        public event Action OnRewardDisplayFail = delegate { };
        public event Action OnInterstitialHidden = delegate { };
        public event Action OnInterstitialDisplayFail = delegate { };
        public bool IsRewardedLoaded => rewarded.IsLoaded;
        string LastTimeWatch => nameof(LastTimeWatch);
        public int secondsPassed;
        public bool useInterstitialTimer;
        public int interstitialRateSec = 60;

        public bool IsInterstitialReady
        {
            get
            {
                if (Keys.IsNoAdsPurchased)
                {
                    //  Debug.LogError("NO ADS");
                    return false;
                }

                if (!useInterstitialTimer)
                {
                    //  Debug.LogError("Is loaded: " + interstitial.IsLoaded);
                    return interstitial.IsLoaded;
                }

                secondsPassed = DateUtils.SecondsPassed(LastTimeWatch);
                // Debug.LogError("Seconds pass: " + secondsPassed + ", " + interstitialRateSec);
                return secondsPassed > interstitialRateSec && interstitial.IsLoaded;
            }
        }

        void Start()
        {
            InitSdk();
            InitModules();
            SubscribeEvents();

            MaxSdk.SetHasUserConsent(false);
            MaxSdk.SetIsAgeRestrictedUser(false);

            if (PlayerPrefs.HasKey("APPLOVIN_FRIST_LAUNCH")) return;
            PlayerPrefs.SetInt("APPLOVIN_FRIST_LAUNCH", 1);
            ResetInterstitialTimer();
        }

        //  [NaughtyAttributes.Button]
        public void ShowRewarded(string msg)
        {
            rewarded.Show(msg);
        }

        string InterstitialAmountKey => nameof(InterstitialWatchedAmount);
        int InterstitialWatchedAmount => PlayerPrefs.GetInt(InterstitialAmountKey, 0);

        //  [NaughtyAttributes.Button()]
        public void ShowInterstitial(string msg)
        {
            if (IsInterstitialReady)
            {
                var amount = InterstitialWatchedAmount;
                if (amount + 1 < 2) //config.showRemoveAdsAfter)
                {
                    amount++;
                    PlayerPrefs.SetInt(InterstitialAmountKey, amount);
                    ResetInterstitialTimer();
                    interstitial.Show(msg);
                    // var startEvent = new AdjustEvent("d0y1pr");
                    // Adjust.trackEvent(startEvent);
                }
                else
                {
                    Invoke(nameof(ShowRemoveAds), 1f);
                }
            }
            else
            {
                Debug.LogWarning("INTERSTITIAL NOT READY");
            }
        }

        public void ResetInterstitialTimer()
            => DateUtils.Save(LastTimeWatch, DateTime.Now);

        void ShowRemoveAds()
        {
            PlayerPrefs.SetInt(InterstitialAmountKey, 0);
            // Game.Instance.gameplay.UI.shops.removeAds.Show();
            // Game.Instance.gameplay.UI.shops.removeAds.OnHide += RemoveAdsHidden;
            //
            // EventManager.Instance.OnProductPurchase += GoNextLevel;
            // EventManager.Instance.OnProductPurchaseFAIL += GoNextLevel;
        }

        void RemoveAdsHidden()
        {
            interstitial.Show("Next Level INTER");
        }

        void OnDisable()
        {
            // if (!EventManager.Instance) return;
            // EventManager.Instance.OnProductPurchase -= GoNextLevel;
        }

        //   void GoNextLevel(string msg) => NextLevel();
        //  void NextLevel() => Game.Instance.NextLevel();

        void InitSdk()
        {
            MaxSdk.SetSdkKey("6PWnckW07ZPBQRITcAKaEqM8wsrrCNGvNDhnCRVqs30D8yG0MJUok-D8DVmW053UB2wV-bTt-eA9A5ZB8R3m0P");
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
            //MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
            //    Debug.LogError("AYAAAAAAAAAAAAAAAA");
            //    // AppLovin SDK is initialized, start loading ads
            //};
        }

        void InitModules()
        {
            rewarded.Init(rewardedID);
            interstitial.Init(interstitialID);
        }

        void SubscribeEvents()
        {
            rewarded.OnRewardReceived += RewardReceived;
            rewarded.OnDisplayFailed += () => OnRewardDisplayFail();
            interstitial.OnHiden += () => OnInterstitialHidden();
            interstitial.OnDisplayFailed += () => OnInterstitialDisplayFail();
        }

        void RewardReceived()
        {
            var startEvent = new AdjustEvent("d0y1pr");
            Adjust.trackEvent(startEvent);
            ResetInterstitialTimer();
            OnRewardReceived();

            var amount = PlayerPrefs.GetInt("REWARD_ADS_WATCHED", 0);
            amount++;
            PlayerPrefs.SetInt("REWARD_ADS_WATCHED", amount);

            if (amount % 10 == 0)
            {
                var e = new AdjustEvent("mk7pqt");
                Adjust.trackEvent(e);
            }
        }
    }
}