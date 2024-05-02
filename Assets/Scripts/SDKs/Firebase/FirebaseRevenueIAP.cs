using UnityEngine;

public class FirebaseRevenueIAP : MonoBehaviour
{
    void Start()
    {
        //   EventManager.Instance.OnProductPurchase += Purchase;
    }

    void OnDisable()
    {
        //   if (!EventManager.Instance) return;
        //  EventManager.Instance.OnProductPurchase -= Purchase;
    }

    void Purchase(string obj)
    {
        //TODO: IT SAYS FIREBASE GET IAP REVENUE BY DEFAULT IF YOU LINK YOUR GOOLE PLAY PROJECT TO IT

        //var impressionParameters = new[]
        //{
        //     new Parameter(ParameterPrice, "AppLovin"),
        //     new Parameter(ParameterCurrency, impressionData.NetworkName)
        // };
        //FirebaseAnalytics.LogEvent(EventPurchase, impressionParameters);
    }
}