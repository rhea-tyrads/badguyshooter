using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{

    public class EnemyDetector : MonoBehaviour
    {
        [SerializeField] float checkDelay = 1f;

        SphereCollider _detectorCollider;
        public SphereCollider DetectorCollider => _detectorCollider;

        int _detectedEnemiesCount;
        List<BaseEnemyBehavior> _detectedEnemies;
        public List<BaseEnemyBehavior> DetectedEnemies => _detectedEnemies;

        BaseEnemyBehavior _closestEnemy;
        public BaseEnemyBehavior ClosestEnemy => _closestEnemy;

        public float DetectorRadius => _detectorCollider.radius;

        float _nextClosestCheckTime = 0.0f;

        IEnemyDetector _enemyDetector;

        public void Initialise(IEnemyDetector enemyDetector)
        {
            this._enemyDetector = enemyDetector;

            // Get detector collider
            _detectorCollider = GetComponent<SphereCollider>();

            // Prepare variables
            _detectedEnemies = new List<BaseEnemyBehavior>();
            _detectedEnemiesCount = 0;

            // Subscribe to enemy dying callback
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        public void SetRadius(float radius)
        {
            _detectorCollider.radius = radius;
        }

        void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            RemoveEnemy(enemy);
        }

        void UpdateClosestEnemy()
        {
            if (_detectedEnemiesCount == 0)
            {
                if (_closestEnemy != null)
                    _enemyDetector.OnCloseEnemyChanged(null);

                _closestEnemy = null;

                return;
            }

            var minDistanceSqr = float.MaxValue;
            BaseEnemyBehavior tempEnemy = null;

            for (var i = 0; i < _detectedEnemiesCount; i++)
            {
                var enemy = _detectedEnemies[i];

                var distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;

                if (distanceSqr < minDistanceSqr && !enemy.IsDead)
                {
                    tempEnemy = enemy;
                    minDistanceSqr = distanceSqr;
                }
            }

            if (_closestEnemy != tempEnemy)
                _enemyDetector.OnCloseEnemyChanged(tempEnemy);

            _closestEnemy = tempEnemy;
        }

        void Update()
        {
            if (_detectedEnemiesCount > 1 && Time.time > _nextClosestCheckTime)
            {
                _nextClosestCheckTime = Time.time + checkDelay;
                UpdateClosestEnemy();
            }
        }

        void RemoveEnemy(BaseEnemyBehavior enemy)
        {
            var enemyIndex = _detectedEnemies.FindIndex(x => x == enemy);
            if (enemyIndex == -1) return;
           
            _detectedEnemies.RemoveAt(enemyIndex);
            _detectedEnemiesCount--;

            UpdateClosestEnemy();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY)) return;
            var enemy = other.GetComponent<BaseEnemyBehavior>();
            if (enemy == null) return;
            if (_detectedEnemies.Contains(enemy)) return;
                
            _detectedEnemies.Add(enemy);
            _detectedEnemiesCount++;

            UpdateClosestEnemy();
        }

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            if (_detectedEnemies.Contains(enemy))
            {
                UpdateClosestEnemy();
            }
            else
            {
                if (!(Vector3.Distance(enemy.transform.position, transform.position) <= DetectorRadius)) return;
                _detectedEnemies.Add(enemy);
                _detectedEnemiesCount++;

                UpdateClosestEnemy();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY)) return;
            var enemy = other.GetComponent<BaseEnemyBehavior>();
            if (enemy != null)
                RemoveEnemy(enemy);
        }

        public void ClearZombiesList()
        {
            _detectedEnemies.Clear();
            UpdateClosestEnemy();
        }

        void OnDestroy()
        {
            BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;
        }

        public void Reload()
        {
            _detectedEnemies.Clear();
            _detectedEnemiesCount = 0;
            _closestEnemy = null;
        }
    }
}
