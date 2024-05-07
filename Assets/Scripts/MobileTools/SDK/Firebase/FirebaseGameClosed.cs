using Firebase.Analytics;
using UnityEngine;

namespace MobileTools.SDK.Firebase
{
    public class FirebaseGameClosed : MonoBehaviour
    {
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // The game is pausing
                Debug.Log("Game has been paused (switched to background or another app)");
                SendEvent();
            }
            else
            {
                // The game is resuming
                Debug.Log("Game has resumed (coming from background)");
            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after user quit");
            SendEvent();
        }

        void SendEvent()
        {
            //  var lvl = LevelsController.Instance.CurrentLevelNumber;
            var lvl = 0;
            FirebaseAnalytics.LogEvent(Events.EXIT_THE_GAME, Params.NUMBER, lvl);
        }
    }
}
