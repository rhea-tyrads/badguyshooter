using UnityEngine;

namespace MobileTools.Utilities
{
    public static class Keys
    {
        const string NO_ADS_PURCHASED = nameof(NO_ADS_PURCHASED);
        const string VIBRO = nameof(VIBRO);
        const string RATE_GAME = nameof(RATE_GAME);
        const string SPECIAL_OFFER = nameof(SPECIAL_OFFER);

        public static bool IsNoAdsPurchased => PlayerPrefs.HasKey(NO_ADS_PURCHASED);
        public static bool IsVibrate => PlayerPrefs.GetInt(VIBRO, 0) == 1;
        public static bool IsRated => PlayerPrefs.GetInt(RATE_GAME, 0) == 1;
        public static bool IsSpecialOfferPurchased => PlayerPrefs.GetInt(SPECIAL_OFFER, 0) == 1;

        public static void PurchaseNoAds() => PlayerPrefs.SetInt(NO_ADS_PURCHASED, 1);
        public static void SetVibro(bool isEnable) => PlayerPrefs.SetInt(VIBRO, isEnable ? 1 : 0);
        public static void Rate() => PlayerPrefs.SetInt(RATE_GAME, 1);
        public static void PurchaseSpecialOffer() => PlayerPrefs.SetInt(SPECIAL_OFFER, 1);

    }
}