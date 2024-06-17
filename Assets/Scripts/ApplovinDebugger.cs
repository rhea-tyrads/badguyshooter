

 
using UnityEngine;
using UnityEngine.UI;

public class ApplovinDebugger : MonoBehaviour
{
    public Button showDebugger;

    public bool test;

    void Start()
    {
        showDebugger.onClick.AddListener(ShowDebugger);
    }

   
    void ShowDebugger()
    {
        MaxSdk.ShowMediationDebugger();
    }
}
