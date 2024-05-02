using Firebase.Analytics;
using System;
using UnityEngine;

public class FirebaseFirstSessionPlaytime : MonoBehaviour
{

    const string SaveKey = "FirstSessionPlaytime";
    bool firstSession;

    void Start()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            PlayerPrefs.SetString(SaveKey, DateTime.Now.ToString());
            firstSession = true;
        }
    }
    void OnApplicationPause(bool pauseStatus)
    {
        if (!firstSession) return;

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
        if (!firstSession) return;

        Debug.Log("Application ending after user quit");
        SendEvent();
    }

    void SendEvent()
    {
        var firstOpen = PlayerPrefs.GetString(SaveKey);
        var date = DateTime.Parse(firstOpen);
        var span = DateTime.UtcNow - date;
        var seconds = span.Seconds;

        FirebaseAnalytics.LogEvent(Events.FIRST_SESSION_DURATION, Params.SECONDS, seconds);
    }
}
