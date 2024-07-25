using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] EnemiesDatabase database;
        public static EnemiesDatabase Database => _instance.database;
        static EnemyController _instance;
        
        // used only by Creatives to ignore attack after damage
        public static readonly bool IgnoreAttackAfterDamage = false; 
      

        public void Initialise()
        {
            _instance = this;
            var baseCharacter = CharactersController.Get(CharacterType.Character01);
            database.InitialiseStatsRealation(baseCharacter.Upgrades[0].Stats.Health);
        }

        // set current character and weapon data - to be used in stats calculation for enemies that will be spawned in a moment
        public static void OnLevelWillBeStarted()
        {
            var characterStats = CharactersController.SelectedCharacter.GetCurrentUpgrade().Stats;
            Database.SetCurrentCharacterStats(characterStats.Health, CharacterBehaviour.GetBehaviour().Weapon.damage.Lerp(0.5f));
        }
    }
}