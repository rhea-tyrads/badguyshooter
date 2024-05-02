using CoreUI;
using System.Collections;
using UnityEngine;
using NetworkReachability = UnityEngine.NetworkReachability;

public class InternetConnection : MonoBehaviour
{

    public bool hasConnection;
    public ScreenUI ui;
    IEnumerator Start()
    {
        while (true)
        {
            var reachability = Application.internetReachability;
            switch (reachability)
            {
                case NetworkReachability.NotReachable:
                    Debug.Log("No internet connection available.");
                    NoConnection();
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    Debug.Log("Connected to mobile data network.");
                    HasConnection();
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    Debug.Log("Connected to Wi-Fi network.");
                    HasConnection();
                    break;
            }

            yield return new WaitForSeconds(5f);
        }
    }

    void HasConnection()
    {
        hasConnection = true;
        ui.Hide();
    }

    void NoConnection()
    {
        hasConnection = false;
        ui.Show();
    }
}