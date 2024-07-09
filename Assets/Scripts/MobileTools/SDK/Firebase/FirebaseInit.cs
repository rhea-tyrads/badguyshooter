using UnityEngine;
using static Firebase.FirebaseApp;

namespace MobileTools.SDK.Firebase
{
    public class FirebaseInit : MonoBehaviour
    {
        global::Firebase.FirebaseApp _App;

        void Start()
        {
            CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == global::Firebase.DependencyStatus.Available)
                    _App = DefaultInstance;
                else
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            });
 
        }
    }
}