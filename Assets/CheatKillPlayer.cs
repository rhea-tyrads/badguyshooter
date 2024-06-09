using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

public class CheatKillPlayer : MonoBehaviour
{
    public bool use;

    void Update()
    {
        if (!use) return;
        use = false;
        LevelController.characterBehaviour.TakeDamage(9999);
    }
}