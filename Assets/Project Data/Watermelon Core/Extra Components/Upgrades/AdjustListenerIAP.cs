
using com.adjust.sdk;

using UnityEngine;

public class AdjustListenerIAP : MonoBehaviour
{
    string IAP_1_gold_250;
    string IAP_2_gold_750;
    string IAP_3_gold_1600;
    string IAP_4_gold_3600;
    string IAP_5_gold_8000;
    string IAP_6_no_ads;
    string gold_250;
    string gold_750;
    string gold_1600;
    string gold_3600;
    string gold_8000;
    string no_ads;

    void Start()
    {

          IAP_1_gold_250 = "kpanh8";
          IAP_2_gold_750 = "4l51v9";
          IAP_3_gold_1600 = "y1wiv0";
          IAP_4_gold_3600 = "76vg1g";
          IAP_5_gold_8000 = "9ix5sm";
          IAP_6_no_ads = "4jnl60";
          gold_250 = "gold_250";
          gold_750 = "gold_750";
          gold_1600 = "gold_1600";
          gold_3600 = "gold_3600";
          no_ads = "no_ads";
        
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
        if (productID == gold_250)
        {
            token = IAP_1_gold_250;
            amount = 16000;
        }

        if (productID == gold_750)
        {
            token = IAP_2_gold_750;
            amount = 47000;
        }

        if (productID == gold_1600)
        {
            token = IAP_3_gold_1600;
            amount = 78000;
        }

        if (productID == gold_3600)
        {
            token = IAP_4_gold_3600;
            amount = 159000;
        }

        if (productID == gold_8000)
        {
            token = IAP_5_gold_8000;
            amount = 299000;
        }

        if (productID == no_ads)
        {
            token = IAP_6_no_ads;
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