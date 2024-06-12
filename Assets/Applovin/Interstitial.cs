using System;
using UnityEngine;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Applovin
{
    public class Interstitial : MonoBehaviour
    {
        string _adUnitID = "8925df940997a3f3";
        int _retryAttempt;
        string _adUnitId;
        public bool IsLoaded { get; private set; }
        public event Action OnHiden = delegate { };
        public event Action OnDisplayFailed = delegate { };
        public void Init(string adUnitID)
        {
            _adUnitID = adUnitID;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnLoad;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnLoadFail;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnClick;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnDisplay;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnDisplayFail;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnHide;

            Load();
        }
        private void OnDisable()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnLoad;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnLoadFail;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnClick;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnDisplay;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnDisplayFail;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnHide;
        }

        void Load()
        {
           MaxSdk.LoadInterstitial(_adUnitID);
        }

        public void Show(string msg)
        {
            MaxSdk.ShowInterstitial(_adUnitId, msg);
            _retryAttempt = 0;
        }

        void OnLoad(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _adUnitId = adUnitId;
            IsLoaded = true;
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
        }

        void OnLoadFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            IsLoaded = false;
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

            _retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

            Invoke(nameof(Load), (float) retryDelay);
        }

        void OnDisplay(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        void OnDisplayFail(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            OnDisplayFailed();
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            Load();
        }

        void OnClick(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        void OnHide(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnHiden();
            Load();
            // Interstitial ad is hidden. Pre-load the next ad.
        }
    }
}