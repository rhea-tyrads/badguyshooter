using Firebase.Extensions;
using UnityEngine;
 using static Firebase.FirebaseApp;

public class FirebaseInit : MonoBehaviour
{
    Firebase.FirebaseApp _app;

    void Start()
    {
        CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
                _app = DefaultInstance;
            else
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        });
 
    }
}