using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class LavaBulletBehavior : PlayerBulletBehavior
    {
        readonly static int SplashParticleHash = ParticlesController.GetHash("Lava Hit");
        readonly static int WallSplashParticleHash = ParticlesController.GetHash("Lava Wall Hit");

        float _explosionRadius;
        DuoInt _damageValue;
        CharacterBehaviour _characterBehaviour;

        TweenCase _movementTween;

        Vector3 _position;
        Vector3 _prevPosition;

        public void Initialise(DuoInt damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float shootingRadius, CharacterBehaviour characterBehaviour, DuoFloat bulletHeight, float explosionRadius)
        {
            base.Initialise(0f, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            this._explosionRadius = explosionRadius;
            this._characterBehaviour = characterBehaviour;

            var targetPosition = currentTarget.transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f));

            var distanceMultiplier = Mathf.InverseLerp(0, shootingRadius, Vector3.Distance(characterBehaviour.transform.position, targetPosition));
            var bulletFlyTime = 1 / speed;

            _damageValue = damage;

            _movementTween = transform.DOBezierMove(targetPosition, Mathf.Lerp(bulletHeight.firstValue, bulletHeight.secondValue, distanceMultiplier), 0, bulletFlyTime).OnComplete(delegate
            {
                OnEnemyHitted(null);
            });

            Tween.DelayedCall(bulletFlyTime * 0.8f, () =>
            {
                AudioController.Play(AudioController.Sounds.shotLavagun, 0.6f);
            });
        }

        void Update()
        {
            _prevPosition = _position;
            _position = transform.position;
        }

        protected override void FixedUpdate()
        {

        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            _movementTween.KillActive();

            var hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);

            foreach (var col in hitColliders)
            {
                if (col.gameObject.layer != PhysicsHelper.LAYER_ENEMY) continue;
                var enemy = col.GetComponent<BaseEnemyBehavior>();
                if (enemy == null || enemy.IsDead) continue;
                var explosionDamageMultiplier = 1.0f - Mathf.InverseLerp(0, _explosionRadius, Vector3.Distance(transform.position, col.transform.position));
                enemy.TakeDamage(CharacterBehaviour.NoDamage ? 0 : _damageValue.Lerp(explosionDamageMultiplier), transform.position, (transform.position - _prevPosition).normalized);
                _characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
            }

            AudioController.Play(AudioController.Sounds.explode);
            gameObject.SetActive(false);
            ParticlesController.Play(SplashParticleHash).SetPosition(transform.position);
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            ParticlesController.Play(WallSplashParticleHash).SetPosition(transform.position);
        }
    }
}