using UnityEngine;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Applovin
{
    public class Banner : MonoBehaviour
    {
        const string BANNER_AD_UNIT_ID = "c1586ad841a06c22"; // Retrieve the ID from your account

        public void Init()
        {
            //MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
            //{
            //    // AppLovin SDK is initialized, start loading ads
            //    // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            //    // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            //    MaxSdk.CreateBanner(BANNER_AD_UNIT_ID, MaxSdkBase.BannerPosition.BottomCenter);

            //    // Set background or background color for banners to be fully functional
            //    MaxSdk.SetBannerBackgroundColor(BANNER_AD_UNIT_ID, Color.black);
            //    MaxSdk.ShowBanner(BANNER_AD_UNIT_ID);
            //};
        }
    }
}