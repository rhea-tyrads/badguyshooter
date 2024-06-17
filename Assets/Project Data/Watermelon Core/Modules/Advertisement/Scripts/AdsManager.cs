#pragma warning disable 0649
#pragma warning disable 0162

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Applovin;

namespace Watermelon
{
    public static class AdsManager
    {
        const int INIT_ATTEMPTS_AMOUNT = 30;

        public const ProductKeyType NO_ADS_PRODUCT_KEY = ProductKeyType.NoAds;

        const string FIRST_LAUNCH_PREFS = "FIRST_LAUNCH";

        const string NO_ADS_PREF_NAME = "ADS_STATE";
        const string NO_ADS_ACTIVE_HASH = "809d08040da0182f4fffa4702095e69e";

        const string GDPR_PREF_NAME = "GDPR_STATE";

        static readonly AdProviderHandler[] AD_PROVIDERS = {
            new AdDummyHandler(AdProvider.Dummy),

#if MODULE_ADMOB
            new AdMobHandler(AdProvider.AdMob),
#endif

#if MODULE_UNITYADS
            new UnityAdsLegacyHandler(AdProvider.UnityAdsLegacy),
#endif

#if MODULE_IRONSOURCE
            new IronSourceHandler(AdProvider.IronSource),
#endif
        };

        static bool isModuleInitialised;

        static AdsSettings settings;
        public static AdsSettings Settings => settings;

        static double lastInterstitialTime;

        static AdProviderHandler.RewardedVideoCallback rewardedVideoCallback;
        static AdProviderHandler.InterstitialCallback interstitalCallback;

        static List<SimpleCallback> mainThreadEvents = new();
        static int mainThreadEventsCount;

        static bool isFirstAdLoaded = false;
        static bool waitingForRewardVideoCallback;

        static bool isBannerActive = true;

        static Coroutine loadingCoroutine;

        static bool isForcedAdEnabled;

        static Dictionary<AdProvider, AdProviderHandler> advertisingActiveModules = new();

        static AdsManagerInitModule initModule;
        public static AdsManagerInitModule InitModule => initModule;

        // Events
        public static event SimpleCallback ForcedAdDisabled;

        public static event AdsModuleCallback AdProviderInitialised;
        public static event AdsEventsCallback AdLoaded;
        public static event AdsEventsCallback AdDisplayed;
        public static event AdsEventsCallback AdClosed;

        public static AdsBoolCallback InterstitialConditions;

        #region Initialise

        public static void Initialise(AdsManagerInitModule adsManagerInitModule, bool loadOnStart)
        {
            if (isModuleInitialised)
            {
                Debug.LogWarning("[AdsManager]: Module already exists!");

                return;
            }

            isModuleInitialised = true;
            isFirstAdLoaded = false;

            initModule = adsManagerInitModule;
            settings = adsManagerInitModule.Settings;

            isForcedAdEnabled = IsForcedAdEnabled(false);

            if (settings == null)
            {
                Debug.LogError("[AdsManager]: Settings don't exist!");

                return;
            }

            if (!PlayerPrefs.HasKey(FIRST_LAUNCH_PREFS))
            {
                lastInterstitialTime = Time.time + settings.InterstitialFirstStartDelay;

                PlayerPrefs.SetInt(FIRST_LAUNCH_PREFS, 1);
            }
            else
            {
                lastInterstitialTime = Time.time + settings.InterstitialStartDelay;
            }

            Initialiser.InitialiserGameObject.AddComponent<AdsManager.AdEventExecutor>();

            advertisingActiveModules = new Dictionary<AdProvider, AdProviderHandler>();
            for (var i = 0; i < AD_PROVIDERS.Length; i++)
            {
                if (IsModuleEnabled(AD_PROVIDERS[i].ProviderType))
                {
                    advertisingActiveModules.Add(AD_PROVIDERS[i].ProviderType, AD_PROVIDERS[i]);
                }
            }

            if (settings.SystemLogs)
            {
                if (settings.BannerType != AdProvider.Disable &&
                    !advertisingActiveModules.ContainsKey(settings.BannerType))
                    Debug.LogWarning("[AdsManager]: Banner type (" + settings.BannerType +
                                     ") is selected, but isn't active!");

                if (settings.InterstitialType != AdProvider.Disable &&
                    !advertisingActiveModules.ContainsKey(settings.InterstitialType))
                    Debug.LogWarning("[AdsManager]: Interstitial type (" + settings.InterstitialType +
                                     ") is selected, but isn't active!");

                if (settings.RewardedVideoType != AdProvider.Disable &&
                    !advertisingActiveModules.ContainsKey(settings.RewardedVideoType))
                    Debug.LogWarning("[AdsManager]: Rewarded Video type (" + settings.RewardedVideoType +
                                     ") is selected, but isn't active!");
            }

            IAPManager.OnPurchaseComplete += OnPurchaseComplete;

            // Add loading task if GDPR isn't created
            if (settings.IsGDPREnabled && !IsGDPRStateExist())
            {
                var gdprLoadingTask = new GDPRLoadingTask();
                gdprLoadingTask.OnTaskCompleted += () => { InitialiseModules(loadOnStart); };

                GameLoading.AddTask(gdprLoadingTask);

                return;
            }

            InitialiseModules(loadOnStart);
        }

        static void InitialiseModules(bool loadAds)
        {
            foreach (var advertisingModule in advertisingActiveModules.Keys)
            {
                InitialiseModule(advertisingModule);
            }

            if (loadAds)
            {
                TryToLoadFirstAds();
            }
        }

        static void InitialiseModule(AdProvider advertisingModule)
        {
            if (advertisingActiveModules.ContainsKey(advertisingModule))
            {
                if (!advertisingActiveModules[advertisingModule].IsInitialised())
                {
                    if (settings.SystemLogs)
                        Debug.Log("[AdsManager]: Module " + advertisingModule + " trying to initialise!");

                    advertisingActiveModules[advertisingModule].Initialise(settings);
                }
                else
                {
                    if (settings.SystemLogs)
                        Debug.Log("[AdsManager]: Module " + advertisingModule + " is already initialised!");
                }
            }
            else
            {
                if (settings.SystemLogs)
                    Debug.LogWarning("[AdsManager]: Module " + advertisingModule + " is disabled!");
            }
        }

        #endregion

        static void Update()
        {
            if (mainThreadEventsCount > 0)
            {
                for (var i = 0; i < mainThreadEventsCount; i++)
                {
                    mainThreadEvents[i]?.Invoke();
                }

                mainThreadEvents.Clear();
                mainThreadEventsCount = 0;
            }

            if (!settings.AutoShowInterstitial) return;
            if (!(lastInterstitialTime < Time.time)) return;
            ShowInterstitial(null);
            ResetInterstitialDelayTime();
        }

        public static void TryToLoadFirstAds()
        {
            if (loadingCoroutine == null)
                loadingCoroutine = Tween.InvokeCoroutine(TryToLoadAdsCoroutine());
        }

        static IEnumerator TryToLoadAdsCoroutine()
        {
            var initAttemps = 0;

            yield return new WaitForSeconds(1.0f);

            while (!isFirstAdLoaded || initAttemps > INIT_ATTEMPTS_AMOUNT)
            {
                if (LoadFirstAds())
                    break;

                yield return new WaitForSeconds(1.0f * (initAttemps + 1));

                initAttemps++;
            }

            if (settings.SystemLogs)
                Debug.Log("[AdsManager]: First ads have loaded!");
        }

        static bool LoadFirstAds()
        {
            if (isFirstAdLoaded)
                return true;

            if (settings.IsGDPREnabled && !AdsManager.IsGDPRStateExist())
                return false;

            if (settings.IsIDFAEnabled && !AdsManager.IsIDFADetermined())
                return false;

            var isRewardedVideoModuleInititalized =
                AdsManager.IsModuleInititalized(AdsManager.Settings.RewardedVideoType);
            var isInterstitialModuleInitialized = AdsManager.IsModuleInititalized(AdsManager.Settings.InterstitialType);
            var isBannerModuleInitialized = AdsManager.IsModuleInititalized(AdsManager.Settings.BannerType);

            var isRewardedVideoActive = AdsManager.Settings.RewardedVideoType != AdProvider.Disable;
            var isInterstitialActive = AdsManager.Settings.InterstitialType != AdProvider.Disable;
            var isBannerActive = AdsManager.Settings.BannerType != AdProvider.Disable;

            if ((!isRewardedVideoActive || isRewardedVideoModuleInititalized) &&
                (!isInterstitialActive || isInterstitialModuleInitialized) &&
                (!isBannerActive || isBannerModuleInitialized))
            {
                if (isRewardedVideoActive)
                    AdsManager.RequestRewardBasedVideo();

                var isForcedAdEnabled = AdsManager.IsForcedAdEnabled(false);
                if (isInterstitialActive && isForcedAdEnabled)
                    AdsManager.RequestInterstitial();

                if (isBannerActive && isForcedAdEnabled)
                    AdsManager.ShowBanner();

                isFirstAdLoaded = true;

                return true;
            }

            return false;
        }

        public static void CallEventInMainThread(SimpleCallback callback)
        {
            if (callback != null)
            {
                mainThreadEvents.Add(callback);
                mainThreadEventsCount++;
            }
        }

        public static void ShowErrorMessage()
        {
            FloatingMessage.ShowMessage("Network error. Please try again later");
        }

        public static bool IsModuleEnabled(AdProvider advertisingModule)
        {
            if (advertisingModule == AdProvider.Disable)
                return false;

            return (Settings.BannerType == advertisingModule || Settings.InterstitialType == advertisingModule ||
                    Settings.RewardedVideoType == advertisingModule);
        }

        public static bool IsModuleActive(AdProvider advertisingModule)
        {
            return advertisingActiveModules.ContainsKey(advertisingModule);
        }

        public static bool IsModuleInititalized(AdProvider advertisingModule)
        {
            if (advertisingActiveModules.ContainsKey(advertisingModule))
            {
                return advertisingActiveModules[advertisingModule].IsInitialised();
            }

            return false;
        }

        #region Interstitial

        public static bool IsInterstitialLoaded()
        {
            return ApplovinController.Instance.IsInterstitialReady;
            return IsInterstitialLoaded(settings.InterstitialType);
        }

        public static bool IsInterstitialLoaded(AdProvider advertisingModules)
        {
            return ApplovinController.Instance.IsInterstitialReady;
            if (!isForcedAdEnabled || !IsModuleActive(advertisingModules))
                return false;

            return advertisingActiveModules[advertisingModules].IsInterstitialLoaded();
        }

        public static void RequestInterstitial()
        {
            return;
            var advertisingModules = settings.InterstitialType;

            if (!isForcedAdEnabled || !IsModuleActive(advertisingModules) ||
                !advertisingActiveModules[advertisingModules].IsInitialised() ||
                advertisingActiveModules[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingActiveModules[advertisingModules].RequestInterstitial();
        }

        public static void ShowInterstitial(AdProviderHandler.InterstitialCallback callback,
            bool ignoreConditions = false)
        {
            return;
            ApplovinController.Instance.ShowInterstitial("inter");
            return;
            var advertisingModules = settings.InterstitialType;

            interstitalCallback = callback;

            if (!isForcedAdEnabled || !IsModuleActive(advertisingModules) ||
                (!ignoreConditions && (!CheckInterstitialTime() || !CheckExtraInterstitialCondition())) ||
                !advertisingActiveModules[advertisingModules].IsInitialised() ||
                !advertisingActiveModules[advertisingModules].IsInterstitialLoaded())
            {
                ExecuteInterstitialCallback(false);

                return;
            }

            advertisingActiveModules[advertisingModules].ShowInterstitial(callback);
        }

        public static void ExecuteInterstitialCallback(bool result)
        {
            return;
            if (interstitalCallback != null)
            {
                CallEventInMainThread(() => interstitalCallback.Invoke(result));
            }
        }

        public static void SetInterstitialDelayTime(float time)
        {
            lastInterstitialTime = Time.time + time;
        }

        public static void ResetInterstitialDelayTime()
        {
            lastInterstitialTime = Time.time + settings.InterstitialShowingDelay;
        }

        static bool CheckInterstitialTime()
        {
            if (settings.SystemLogs)
                Debug.Log("[AdsManager]: Interstitial Time: " + lastInterstitialTime + "; Time: " + Time.time);

            return lastInterstitialTime < Time.time;
        }

        public static bool CheckExtraInterstitialCondition()
        {
            if (InterstitialConditions == null) return true;

            var listDelegates = InterstitialConditions.GetInvocationList();
            var state = listDelegates.All(d => (bool) d.DynamicInvoke());
            if (settings.SystemLogs)
                Debug.Log("[AdsManager]: Extra condition interstitial state: " + state);

            return state;
        }

        #endregion

        #region Rewarded Video

        public static bool IsRewardBasedVideoLoaded()
        {
            return ApplovinController.Instance.IsRewardedLoaded;
            var advertisingModule = settings.RewardedVideoType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return false;

            return advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded();
        }

        public static void RequestRewardBasedVideo()
        {
            return;
            var advertisingModule = settings.RewardedVideoType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised() ||
                advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded())
                return;

            advertisingActiveModules[advertisingModule].RequestRewardedVideo();
        }

        public static void ShowRewardBasedVideo(AdProviderHandler.RewardedVideoCallback callback,
            bool showErrorMessage = true)
        {
            if (ApplovinController.Instance.IsRewardedLoaded)
                ApplovinController.Instance.ShowRewarded("rewarded");
            //  rewardedVideoCallback = callback;

            return;
            var advertisingModule = settings.RewardedVideoType;

            rewardedVideoCallback = callback;
            waitingForRewardVideoCallback = true;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised() ||
                !advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded())
            {
                ExecuteRewardVideoCallback(false);

                if (showErrorMessage)
                    ShowErrorMessage();

                return;
            }

            advertisingActiveModules[advertisingModule].ShowRewardedVideo(callback);
        }

        public static void ExecuteRewardVideoCallback(bool result)
        {
            return;
            if (rewardedVideoCallback != null && waitingForRewardVideoCallback)
            {
                CallEventInMainThread(() => rewardedVideoCallback.Invoke(result));

                waitingForRewardVideoCallback = false;

                if (settings.SystemLogs)
                {
                    Debug.Log("[AdsManager]: Reward received: " + result);
                }
            }
        }

        #endregion

        #region Banner

        public static void ShowBanner()
        {
            if (!isBannerActive) return;

            var advertisingModule = settings.BannerType;

            if (!isForcedAdEnabled || !IsModuleActive(advertisingModule) ||
                !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].ShowBanner();
        }

        public static void DestroyBanner()
        {
            var advertisingModule = settings.BannerType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].DestroyBanner();
        }

        public static void HideBanner()
        {
            var advertisingModule = settings.BannerType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].HideBanner();
        }

        public static void EnableBanner()
        {
            isBannerActive = true;

            ShowBanner();
        }

        public static void DisableBanner()
        {
            isBannerActive = false;

            HideBanner();
        }

        #endregion

        public static void OnProviderInitialised(AdProvider advertisingModule)
        {
            AdProviderInitialised?.Invoke(advertisingModule);
        }

        public static void OnProviderAdLoaded(AdProvider advertisingModule, AdType advertisingType)
        {
            AdLoaded?.Invoke(advertisingModule, advertisingType);
        }

        public static void OnProviderAdDisplayed(AdProvider advertisingModule, AdType advertisingType)
        {
            AdDisplayed?.Invoke(advertisingModule, advertisingType);

            if (advertisingType == AdType.Interstitial || advertisingType == AdType.RewardedVideo)
            {
                ResetInterstitialDelayTime();
            }
        }

        public static void OnProviderAdClosed(AdProvider advertisingModule, AdType advertisingType)
        {
            AdClosed?.Invoke(advertisingModule, advertisingType);

            if (advertisingType == AdType.Interstitial || advertisingType == AdType.RewardedVideo)
            {
                ResetInterstitialDelayTime();
            }
        }

        #region IAP

        static void OnPurchaseComplete(ProductKeyType productKeyType)
        {
            if (productKeyType == NO_ADS_PRODUCT_KEY)
            {
                DisableForcedAd();
            }
        }

        public static bool IsForcedAdEnabled(bool useCachedValue = true)
        {
            if (useCachedValue)
                return isForcedAdEnabled;

            return !PlayerPrefs.GetString(NO_ADS_PREF_NAME, "").Equals(NO_ADS_ACTIVE_HASH);
        }

        public static void DisableForcedAd()
        {
            Debug.Log("[Ads Manager]: Banners and interstitials are disabled!");

            PlayerPrefs.SetString(NO_ADS_PREF_NAME, NO_ADS_ACTIVE_HASH);

            isForcedAdEnabled = false;

            ForcedAdDisabled?.Invoke();

            DestroyBanner();
        }

        #endregion

        #region GDPR

        public static void SetGDPR(bool state)
        {
            PlayerPrefs.SetInt(GDPR_PREF_NAME, state ? 1 : 0);

            foreach (var activeModule in advertisingActiveModules.Keys)
            {
                if (advertisingActiveModules[activeModule].IsInitialised())
                {
                    advertisingActiveModules[activeModule].SetGDPR(state);
                }
                else
                {
                    InitialiseModule(activeModule);
                }
            }
        }

        public static bool GetGDPRState()
        {
            return PlayerPrefs.GetInt(GDPR_PREF_NAME, 0) == 1 ? true : false;
        }

        public static bool IsGDPRStateExist()
        {
            return PlayerPrefs.HasKey(GDPR_PREF_NAME);
        }

        #endregion

        #region IDFA

        public static bool IsIDFADetermined()
        {
#if UNITY_IOS
            if(settings.IsIDFAEnabled)
            {
                return Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
            }
#endif

            return true;
        }

        #endregion

        public delegate void AdsModuleCallback(AdProvider advertisingModules);

        public delegate void AdsEventsCallback(AdProvider advertisingModules, AdType advertisingType);

        public delegate bool AdsBoolCallback();

        class AdEventExecutor : MonoBehaviour
        {
            void Update()
            {
                AdsManager.Update();
            }
        }
    }
}

// -----------------
// Advertisement v1.4.2
// -----------------

// Changelog
// v1.4.2
// • Added EnableBanner, DisableBanner methods
// v1.4.1
// • Added ironSource (Unity LevelPlay) ad provider
// v1.4
// • Admob v9.0.0 support
// • Better naming and code cleanup
// • Ads callbacks replaced with simplified ones (AdLoaded, AdDisplayed, AdClosed)
// • Removed ShowInterstitial, ShowRewardedVideo, ShowBanner methods with provider type parameter
// • Added optional bool parameter to ShowInterstitial method. Allows to show interstitial even if conditions aren't met
// v1.3
// • Admob v8.1.0 support
// • Removed IronSource provider
// v1.2.1
// • Some fixes in IronSourse provider
// • Some fixes in Admob provider
// • New interface in Admob provider
// • Added Build Preprocessing for Admob 
// v1.2
// • Added IronSource provider
// v1.1f3
// • GDPR style rework
// • Rewarded video error message
// • Removed GDPR check in AdMob module
// v1.1f2
// • GDPR init bug fixed
// v1.1
// • Added first ad loader
// • Moved IAP check to AdsManager script
// v1.0
// • Added documentation
// v0.3
// • Unity Ads fixed
// v0.2
// • Bug fix
// v0.1
// • Added basic version