using System.Collections;
using UnityEngine;
using NetworkReachability = UnityEngine.NetworkReachability;

namespace MobileTools.InternetCheck
{
    public class InternetConnection : MobileTool
    {

        public bool hasConnection;
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
            mobileUI.Hide();
        }

        void NoConnection()
        {
            hasConnection = false;
            mobileUI.Show();
        }
    }
}