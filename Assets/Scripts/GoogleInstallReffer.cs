using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class GoogleInstallReffer : MonoBehaviour
{
  
    void Start()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

        using var referrerClient = new AndroidJavaObject("com.android.installreferrer.api.InstallReferrerClient$Builder", context);
        var client = referrerClient.Call<AndroidJavaObject>("build");
        client.Call("startConnection", new ReferrerStateListener(client));
    }
    
}
public class ReferrerStateListener : AndroidJavaProxy
{
    AndroidJavaObject client;

    public ReferrerStateListener(AndroidJavaObject client) : base("com.android.installreferrer.api.InstallReferrerStateListener")
    {
        this.client = client;
    }

    void onInstallReferrerSetupFinished(int responseCode)
    {
        if (responseCode == 0) // InstallReferrerResponse.OK
        {
            var referrerDetails = client.Call<AndroidJavaObject>("getInstallReferrer");
            var referrerUrl = referrerDetails.Call<string>("getInstallReferrer");
            Debug.Log("Install Referrer URL: " + referrerUrl);
        }
        else
        {
            Debug.Log("Install Referrer setup failed, response code: " + responseCode);
        }
    }

    void onInstallReferrerServiceDisconnected()
    {
        Debug.Log("Install Referrer service disconnected");
    }
}