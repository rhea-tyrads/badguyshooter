using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

public class testwe : MonoBehaviour
{
    public WeaponsController weapons;

    void Update()
    {
        if (WeaponsController.IsWeaponUnlocked(WeaponType.Shotgun))
        {
            Debug.LogError("AAAAAAAAAAAAAAAAAA");
        }
    }

    void Start()
    {
    }
}