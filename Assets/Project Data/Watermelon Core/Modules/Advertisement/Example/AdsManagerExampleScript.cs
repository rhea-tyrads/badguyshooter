#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class AdsManagerExampleScript : MonoBehaviour
    {
        Vector2 scrollView;

        [SerializeField] Text logText;

        [Space]
        [SerializeField]
        Text bannerTitleText;
        [SerializeField] Button[] bannerButtons;

        [Space]
        [SerializeField]
        Text interstitialTitleText;
        [SerializeField] Button[] interstitialButtons;

        [Space]
        [SerializeField]
        Text rewardVideoTitleText;
        [SerializeField] Button[] rewardVideoButtons;

        AdsSettings settings;

        void Awake()
        {
            Application.logMessageReceived += Log;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= Log;
        }

        void Start()
        {
            settings = AdsManager.Settings;

            logText.text = string.Empty;

            bannerTitleText.text = string.Format("Banner ({0})", settings.BannerType.ToString());
            if(settings.BannerType == AdProvider.Disable)
            {
                for(var i = 0; i < bannerButtons.Length; i++)
                {
                    bannerButtons[i].interactable = false; 
                }
            }

            interstitialTitleText.text = string.Format("Interstitial ({0})", settings.InterstitialType.ToString());
            if (settings.InterstitialType == AdProvider.Disable)
            {
                for (var i = 0; i < interstitialButtons.Length; i++)
                {
                    interstitialButtons[i].interactable = false;
                }
            }

            rewardVideoTitleText.text = string.Format("Rewarded Video ({0})", settings.RewardedVideoType.ToString());
            if (settings.RewardedVideoType == AdProvider.Disable)
            {
                for (var i = 0; i < rewardVideoButtons.Length; i++)
                {
                    rewardVideoButtons[i].interactable = false;
                }
            }

            GameLoading.MarkAsReadyToHide();
        }

        void Log(string condition, string stackTrace, LogType type)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }

        void Log(string condition)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }

        #region Buttons
        public void ShowBannerButton()
        {
            AdsManager.ShowBanner();
        }

        public void HideBannerButton()
        {
            AdsManager.HideBanner();
        }

        public void DestroyBannerButton()
        {
            AdsManager.DestroyBanner();
        }

        public void InterstitialStatusButton()
        {
            Log("[AdsManager]: Interstitial " + (AdsManager.IsInterstitialLoaded() ? "is loaded" : "isn't loaded"));
        }

        public void RequestInterstitialButton()
        {
            AdsManager.RequestInterstitial();
        }

        public void ShowInterstitialButton()
        {
            AdsManager.ShowInterstitial( (isDisplayed) =>
            {
                Debug.Log("[AdsManager]: Interstitial " + (isDisplayed ? "is" : "isn't") + " displayed!");
            }, true);
        }

        public void RewardedVideoStatusButton()
        {
            Log("[AdsManager]: Rewarded video " + (AdsManager.IsRewardBasedVideoLoaded() ? "is loaded" : "isn't loaded"));
        }

        public void RequestRewardedVideoButton()
        {
            AdsManager.RequestRewardBasedVideo();
        }

        public void ShowRewardedVideoButton()
        {
            AdsManager.ShowRewardBasedVideo( (hasReward) =>
            {
                Log(hasReward ? "[AdsManager]: Reward is received" : "[AdsManager]: Reward isn't received");
            });
        }
        #endregion
    }
}