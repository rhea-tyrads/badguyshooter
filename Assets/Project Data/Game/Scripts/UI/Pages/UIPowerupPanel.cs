using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

public class UIPowerupPanel : MonoBehaviour
{
    public GameObject container;
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
        return;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        return;
        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        container.SetActive(GameController.IsGameActive);


        var player = LevelController.characterBehaviour;
        if (!player)
        {
            Debug.LogError("NO PLAYER");
            atkSpd.SetActive(false);
            moveSpd.SetActive(false);
            multishotSpd.SetActive(false);
            return;
        }

        atkSpd.SetActive(player.isAtkSpdBooster);
        moveSpd.SetActive(player.isMoveSpeedBooster);
        multishotSpd.SetActive(player.isMultishotBooster);
        hp.SetActive(player.isHpBonus);
        crit.SetActive(player.IsCritical);
        respawn.SetActive(player.respawnCount > 0);
    }

    public void SetBonuses(BonusUI ui)
    {
//         Debug.LogError(ui,ui.gameObject);
        hp.SetActive(ui.IsHpActive);
        crit.SetActive(ui.critBonus);
        respawn.SetActive(ui.respawnBonus);
    }
}