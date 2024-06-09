
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class CheatAllWeapons : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(Cheat), 0.1f);
    }

    void Cheat()
    {
        PlayerPrefs.SetInt("WEAPON_CHEAT", 1);
        Debug.LogError("DISABLE WEAPON CHEAT!!!!!!!!!!!!!!!!!!!!!!!!");
        WeaponsController.UnlockAllWeaponsDev();
        UIController.GetPage<UIWeaponPage>().UpdateUI();
    }
 
}
