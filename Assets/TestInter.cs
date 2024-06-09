using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public class TestInter : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Do),5f);
    }

    // Update is called once per frame
    void Do()
    {
        AdsManager.ShowInterstitial(null);
    }
}
