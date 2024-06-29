 
using com.adjust.sdk;
 
using UnityEngine;

public class AdjustADSwatched : MonoBehaviour
{
    string AD_WATCH;

    void Start()
    {
       // DontDestroyOnLoad(gameObject);

        AD_WATCH = "d0y1pr";

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
    }

    void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        var send = new AdjustEvent(AD_WATCH);
        Adjust.trackEvent(send);
        Debug.LogWarning("[Adjust] AD WATCHED, " + "TOKEN: " + AD_WATCH);
    }
}

