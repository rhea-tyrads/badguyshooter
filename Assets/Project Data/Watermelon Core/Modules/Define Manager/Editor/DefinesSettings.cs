namespace Watermelon
{
    public static class DefinesSettings
    {
        public static readonly string[] STATIC_DEFINES = new string[]
        {
            "UNITY_POST_PROCESSING_STACK_V2",
            "PHOTON_UNITY_NETWORKING",
            "PUN_2_0_OR_NEWER",
            "PUN_2_OR_NEWER",
        };

        public static readonly RegisteredDefine[] REGISTERED_DEFINES = new RegisteredDefine[]
        {
            // Ads providers
            new RegisteredDefine("MODULE_ADMOB", "GoogleMobileAds.Editor.GoogleMobileAdsSettings", new string[] { "Assets/GoogleMobileAds/GoogleMobileAds.dll" }),
            new RegisteredDefine("MODULE_UNITYADS", "UnityEngine.Advertisements.Advertisement", new string[] { "Packages/com.unity.ads/Runtime/Advertisement/Advertisement.cs" }),
            new RegisteredDefine("MODULE_IRONSOURCE", "IronSource", new string[] { "Assets/IronSource/Scripts/IronSource.cs", "Assets/LevelPlay/Runtime/IronSource.cs" }),

            // System
            new RegisteredDefine("MODULE_INPUT_SYSTEM", "UnityEngine.InputSystem.InputManager", new string[] { "Packages/com.unity.inputsystem/InputSystem/InputManager.cs" }),

            // Core
            new RegisteredDefine("MODULE_IAP", "UnityEngine.Purchasing.UnityPurchasing", new string[] { "Packages/com.unity.purchasing/Runtime/Purchasing/UnityPurchasing.cs" }),
            new RegisteredDefine("MODULE_POWERUPS", "Watermelon.PUController", new string[] { "Assets/Project Data/Watermelon Core/Extra Components/Power Ups System/Scripts/PUController.cs" }),
        };
    }
}