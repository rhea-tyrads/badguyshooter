using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Enemies Database", menuName = "Content/Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [SerializeField] EnemyData[] enemies;
        public EnemyData[] Enemies => enemies;

        public void InitialiseStatsRealation(int baseCharacterHealth)
        {
            foreach (var enemy in enemies)
            {
                enemy.Stats.InitialiseStatsRelation(baseCharacterHealth);
            }
        }

        public void SetCurrentCharacterStats(int characterHealth, int weaponDmg)
        {
            foreach (var enemy in enemies)
            {
                enemy.Stats.SetCurrentCreatureStats(characterHealth, weaponDmg, BalanceController.GetActiveDifficultySettings());
            }
        }

        public EnemyData GetEnemyData(EnemyType type)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.EnemyType.Equals(type))
                    return enemy;
            }

            Debug.LogError("[Enemies Database] Enemy of type " + type + " + is not found!");
            return enemies[0];
        }
    }
}