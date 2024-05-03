using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SignalCondition : MonoBehaviour
{
    protected SignalUI signal;


    void Awake()
    {
        signal = GetComponent<SignalUI>();
    }

    protected virtual void ForceHide(bool forceHide)
        => signal.ForceHide(forceHide);
}
