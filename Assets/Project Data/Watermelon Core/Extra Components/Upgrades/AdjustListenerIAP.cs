
using com.adjust.sdk;

using UnityEngine;

public class AdjustListenerIAP : MonoBehaviour
{
    string _iap1Gold250;
    string _iap2Gold750;
    string _iap3Gold1600;
    string _iap4Gold3600;
    string _iap5Gold8000;
    string _iap6NoAds;
    string _gold250;
    string _gold750;
    string _gold1600;
    string _gold3600;
    string _gold8000;
    string _noAds;

    void Start()
    {

          _iap1Gold250 = "kpanh8";
          _iap2Gold750 = "4l51v9";
          _iap3Gold1600 = "y1wiv0";
          _iap4Gold3600 = "76vg1g";
          _iap5Gold8000 = "9ix5sm";
          _iap6NoAds = "4jnl60";
          _gold250 = "gold_250";
          _gold750 = "gold_750";
          _gold1600 = "gold_1600";
          _gold3600 = "gold_3600";
          _noAds = "no_ads";
        
       // EventManager.Instance.OnProductPurchase += Purchase;
    }

    void OnDisable()
    {
     //   if (!EventManager.Instance) return;
    //    EventManager.Instance.OnProductPurchase -= Purchase;
    }

    void Purchase(string productID)
    {
        string token = string.Empty;
        double amount = 0;
        string currency = "IDR";
        if (productID == _gold250)
        {
            token = _iap1Gold250;
            amount = 16000;
        }

        if (productID == _gold750)
        {
            token = _iap2Gold750;
            amount = 47000;
        }

        if (productID == _gold1600)
        {
            token = _iap3Gold1600;
            amount = 78000;
        }

        if (productID == _gold3600)
        {
            token = _iap4Gold3600;
            amount = 159000;
        }

        if (productID == _gold8000)
        {
            token = _iap5Gold8000;
            amount = 299000;
        }

        if (productID == _noAds)
        {
            token = _iap6NoAds;
            amount = 119000;
        }

        if (token.Equals(string.Empty)) return;

        var send = new AdjustEvent(token);
        send.setRevenue(amount, currency);
        Adjust.trackEvent(send);
        var logMsg = "[Adjust] IAP Purchase: " + productID + ", TOKEN: " + token;
        Debug.Log(logMsg);
    }
}