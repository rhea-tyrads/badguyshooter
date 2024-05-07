using Firebase.Analytics;
using UnityEngine;

namespace MobileTools.SDK.Firebase
{
    public class FirebaseLevelComplete : MonoBehaviour
    {
        void Start()
        {
            SDKEvents.Instance.OnLevelComplete += LevelComplete;
        }

        void LevelComplete(int worldID, int roomID)
        {
            var world = "World_" + worldID;
            var room = "Room_" + roomID;
            var eventName = "Complete_" + world + "_" + room;
            FirebaseAnalytics.LogEvent(eventName);
        }
    }
}