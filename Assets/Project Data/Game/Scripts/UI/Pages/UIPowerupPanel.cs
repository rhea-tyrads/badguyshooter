using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

public class UIPowerupPanel : MonoBehaviour
{
    public GameObject hp;
    public GameObject crit;
    public GameObject respawn;

    public GameObject atkSpd;
    public GameObject moveSpd;
    public GameObject multishotSpd;

    void Awake()
    {
        hp.SetActive(false);
        crit.SetActive(false);
        respawn.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    void FixedUpdate()
    {
        if (!GameController.IsGameActive)
        {
            atkSpd.SetActive(false);
            moveSpd.SetActive(false);
            multishotSpd.SetActive(false);
        }
        
        var player = LevelController.characterBehaviour;
        if (!player)
        {
            atkSpd.SetActive(false);
            moveSpd.SetActive(false);
            multishotSpd.SetActive(false);
            return;
        }
        
        atkSpd.SetActive(player.isAtkSpdBooster);
        moveSpd.SetActive(player.isMoveSpeedBooster);
        multishotSpd.SetActive(player.isMultishotBooster);
    }

    public void SetBonuses(BonusUI file)
    {
        //Debug.LogError(file.IsHpActive);
        hp.SetActive(file.IsHpActive);
        crit.SetActive(file.critBonus);
        respawn.SetActive(file.respawnBonus);
    }
}