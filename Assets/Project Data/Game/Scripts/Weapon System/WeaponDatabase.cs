using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Weapon Database", menuName = "Content/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        [SerializeField] WeaponData[] weapons;
        public WeaponData[] Weapons => weapons;

        [SerializeField] RarityData[] raritySettings;
        public RarityData[] RaritySettings => raritySettings;

        public WeaponData GetWeapon(WeaponType type)
        {
            foreach (var data in weapons)
            {
                if (data.Type.Equals(type))
                    return data;
            }

            Debug.LogError("Weapon data of type: " + type + " is not found");
            return weapons[0];
        }

        public WeaponData GetWeaponByIndex(int index)
        {
            return weapons[index % weapons.Length];
        }

        public RarityData GetRarity(Rarity rarity)
        {
            for (var i = 0; i < raritySettings.Length; i++)
            {
                if (raritySettings[i].Rarity.Equals(rarity))
                    return raritySettings[i];
            }

            Debug.LogError("Rarity data of type: " + rarity + " is not found");
            return raritySettings[0];
        }
    }
}