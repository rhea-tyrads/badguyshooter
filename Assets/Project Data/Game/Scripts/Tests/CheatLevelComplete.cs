using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

public class CheatLevelComplete : MonoBehaviour
{
    public bool completeLevel;

    void Update()
    {
        if (!completeLevel) return;
        completeLevel = false;
        LevelController.OnPlayerExitLevel();
    }
}