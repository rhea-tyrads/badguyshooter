using System;
using System.Collections.Generic;
using com.adjust.sdk;
using MobileTools.MonoCache.System;
using MobileTools.SDK;
using UnityEngine;
using Watermelon.LevelSystem;

public class AdjustController : MonoBehaviour
{
    public List<string> levelCompleteEvents = new();
    DateTime sessionStartTime;

    void Start()
    {
        var token = "upofu32g0o3k";
        var adjust = new AdjustConfig(token, AdjustEnvironment.Sandbox);
        Adjust.start(adjust);
        StartSession();
        SDKEvents.Instance.OnLevelComplete += LevelComplete;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) EndSession();
        else StartSession();
    }

    void OnApplicationQuit()
    {
        EndSession();
    }

    void StartSession()
    {
        sessionStartTime = DateTime.Now;
        var startEvent = new AdjustEvent("mi8sge");
        Adjust.trackEvent(startEvent);
    }

    void EndSession()
    {
        var sessionEndTime = DateTime.Now;
        var sessionDuration = sessionEndTime - sessionStartTime;
        var endEvent = new AdjustEvent("iay4np");
        endEvent.addCallbackParameter("duration", sessionDuration.TotalSeconds.ToString());
        Adjust.trackEvent(endEvent);
    }

    public void LevelComplete(int world, int level)
    {
        var lvlNumber = 0;
        var token = levelCompleteEvents[lvlNumber];
        var send = new AdjustEvent(token);
        Adjust.trackEvent(send);
    }
}