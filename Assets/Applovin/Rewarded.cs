using System;
using UnityEngine;
 using static MaxSdkBase;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Applovin
{
    public class Rewarded : MonoBehaviour
    {
        string _adUnitID = "055c46acd5d7896b";
        int _retryAttempt;
        string _adUnitId;
        public bool IsLoaded { get; private set; }
        public event Action OnRewardReceived = delegate { };
        public event Action OnDisplayFailed = delegate { };

        public void Init(string adUnitID)
        {
            _adUnitID = adUnitID;


            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnLoad;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnLoadFail;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnDisplay;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnClick;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnHide;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnDisplayFail;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnReward;

            Load();
        }
        private void OnDisable()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnLoad;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnLoadFail;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnDisplay;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnClick;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnHide;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnDisplayFail;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnReward;
        }
        void Load()
        {
            MaxSdk.LoadRewardedAd(_adUnitID);
        }

        void OnLoad(string adUnitId, AdInfo adInfo)
        {
            IsLoaded = true;
            _adUnitId = adUnitId;
            Debug.Log("MAX rewarded ads loaded successfully");
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
        }

        public void Show(string msg)
        {
             MaxSdk.ShowRewardedAd(_adUnitId, msg);
            _retryAttempt = 0;
        }

        void OnLoadFail(string adUnitId, ErrorInfo errorInfo)
        {
            IsLoaded = false;
            // Debug.LogError("Load success");
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            _retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));
            Invoke(nameof(Load), (float)retryDelay);
        }

        void OnDisplay(string adUnitId, AdInfo adInfo)
        {
        }

        void OnDisplayFail(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
        {
            OnDisplayFailed();
            // Rewarded ad failed to display. 
            Load();
        }

        void OnClick(string adUnitId , AdInfo adInfo)
        {
        }

        void OnHide(string adUnitId , AdInfo adInfo)
        {
            //Rewarded ad is hidden. Pre-load the next ad
        }

        void OnReward(string adUnitId , Reward reward, AdInfo adInfo)
        {
            //The rewarded ad displayed and the user should receive the reward.
            OnRewardReceived();
        }

        void OnRevenuePaid(string adUnitId , AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
    }
}