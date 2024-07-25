using System.Collections;
using System.Collections.Generic;
using Applovin;
using UnityEngine;
using Watermelon;

public class TestInter : MonoBehaviour
{
    
 
    void Start()
    {
        Invoke(nameof(Do),3f);
    }

   
    void Do()
    {
        ApplovinController.Instance.ShowInterstitial("inter");
    }
}
