 
using com.adjust.sdk;
 
using UnityEngine;

public class AdjustADSwatched : MonoBehaviour
{
    string _adWatch;

    void Start()
    {
       // DontDestroyOnLoad(gameObject);

        _adWatch = "d0y1pr";

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
    }

    void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        var send = new AdjustEvent(_adWatch);
        Adjust.trackEvent(send);
        Debug.LogWarning("[Adjust] AD WATCHED, " + "TOKEN: " + _adWatch);
    }
}

