using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

public class UIPowerupPanel : MonoBehaviour
{
    public GameObject hp;
    public GameObject crit;
    public GameObject respawn;

    public GameObject atkSpd;
    public GameObject moveSpd;
    public GameObject multishotSpd;

    void FixedUpdate()
    {
        var player = LevelController.characterBehaviour;
        if (!player) return;
        atkSpd.SetActive(player.isAtkSpdBooster);
        moveSpd.SetActive(player.isMoveSpeedBooster);
        multishotSpd.SetActive(player.isMultishotBooster);
    }

    public void SetBonuses(BonusUI file)
    {
        hp.SetActive(file.IsHpActive);
        crit.SetActive(file.critBonus);
        respawn.SetActive(file.respawnBonus);
    }
}