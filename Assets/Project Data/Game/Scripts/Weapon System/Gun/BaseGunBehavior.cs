using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        static readonly int _particleUpgrade = ParticlesController.GetHash("Gun Upgrade");
        [Header("Animations")]
        [SerializeField] AnimationClip characterShootAnimation;
        [Space]
        [SerializeField] Transform leftHandHolder;
        [SerializeField] Transform rightHandHolder;
        [SerializeField] protected ParticleSystem shootParticleSystem;
        [Space]
        [SerializeField] protected Transform shootPoint;
        [Header("Upgrade")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [SerializeField] float upgradeParticleSize = 1.0f;
        [SerializeField] protected LayerMask targetLayers;
        protected CharacterBehaviour CharacterBehaviour;
        [SerializeField] protected List<float> bulletStreamAngles = new() { 0 };
        protected CharacterBehaviour Owner => CharacterBehaviour;
        protected TweenCase _shootTweenCase;
        protected WeaponData Data;
        public DuoInt damage;
        [FormerlySerializedAs("bulletDisableTime")] [SerializeField]
        protected float bulletLifeTime = 1f;
        public float Damage => damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier;
        Transform _leftHandRigController;
        Vector3 _leftHandExtraRotation;
        Transform _rightHandRigController;
        Vector3 _rightHandExtraRotation;
        protected DuoFloat _bulletSpeed;
        protected Vector3 FaceDirection => CharacterBehaviour.transform.eulerAngles;
        protected void TargetUnreachable() => CharacterBehaviour.TargetUnreachable();
        protected float BulletSpeed => _bulletSpeed.Random();
        protected BaseEnemyBehavior Target => CharacterBehaviour.ClosestEnemyBehaviour;
        protected float _nextShootTime;
        protected float _attackDelay;
        protected Vector3 _shootDirection;
        float defaultSpread = 30;
        protected bool NotLookAtTarget
            => Vector3.Angle(_shootDirection, transform.forward.SetY(0f)) > 40f;

        protected float FireRate()
        {
            return Time.timeSinceLevelLoad + _attackDelay / AtkSpdMult;
        }

        protected float AtkSpdMult => CharacterBehaviour.AtkSpdMult;
        protected bool VisionIsClear =>
            // Physics.Raycast(transform.position, _shootDirection, out var hit, 300f, targetLayers)
            Physics.Raycast(transform.position - _shootDirection.normalized, _shootDirection, out var hit, 300f, targetLayers)
            && hit.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY;

        protected void AimAtTarget()
        {
            _shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
        }

        protected bool NotReady => _nextShootTime >= Time.timeSinceLevelLoad;

        public virtual void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            CharacterBehaviour = characterBehaviour;
            Data = data;
        }

        public void InitialiseCharacter(BaseCharacterGraphics characterGraphics)
        {
            _leftHandRigController = characterGraphics.LeftHandRig.data.target;
            _rightHandRigController = characterGraphics.RightHandRig.data.target;
            _leftHandExtraRotation = characterGraphics.LeftHandExtraRotation;
            _rightHandExtraRotation = characterGraphics.RightHandExtraRotation;
            characterGraphics.SetShootingAnimation(characterShootAnimation);
        }

        int bulletsAmount;

        protected int RandomBulletsAmount(BaseWeaponUpgrade upgrade)
        {
            var amount = upgrade.GetCurrentStage().BulletsPerShot.Random() + CharacterBehaviour.MultishotBoosterAmount;
            RefreshAngles(amount);
            return amount;
        }

        void RefreshAngles(int amount)
        {
            if (bulletsAmount == amount) return;
            bulletsAmount = amount;

            bulletStreamAngles.Clear();
            if (amount == 1)
            {
                bulletStreamAngles.Add(0);
                return;
            }

            var step = defaultSpread / (amount - 1);
            var angle = -defaultSpread / 2;
            for (int i = 0; i < amount; i++)
            {
                angle += i * step;
                bulletStreamAngles.Add(angle);
            }
        }

        Vector3 GetAngle(int id)
        {
            var b = id < bulletStreamAngles.Count ? bulletStreamAngles[id] : 0;
            var a = Vector3.up * (Random.Range(-_spread, _spread) + b);
            return a;
        }

        protected bool NoTarget
        {
            get
            {
                if (!CharacterBehaviour.IsCloseEnemyFound) return true;
                if (CharacterBehaviour.ClosestEnemyBehaviour.isInStealth) return true;
                return false;
            }
        }

        public virtual void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public void OnFixedUpdate()
        {
            if (NotReady) return;
            if (NoTarget) return;
            AimAtTarget();
            if (NotLookAtTarget) return;

            if (VisionIsClear)
            {
                _nextShootTime = FireRate();
                Shoot();
            }
            else
            {
                TargetUnreachable();
            }
        }

        protected PooledObjectSettings PoolSettings() => new PooledObjectSettings().SetPosition(shootPoint.position).SetRotation(FaceDirection);
        protected PooledObjectSettings PoolSettings(Vector3 angle) => new PooledObjectSettings().SetPosition(shootPoint.position).SetRotation(FaceDirection + angle);

        public virtual void Shoot()
        {
        }

        public void UpdateHandRig()
        {
            _leftHandRigController.position = leftHandHolder.position;
            _rightHandRigController.position = rightHandHolder.position;

#if UNITY_EDITOR
            if (CharacterBehaviour && CharacterBehaviour.Graphics)
            {
                _leftHandExtraRotation = CharacterBehaviour.Graphics.LeftHandExtraRotation;
                _rightHandExtraRotation = CharacterBehaviour.Graphics.RightHandExtraRotation;
            }
#endif

            _leftHandRigController.rotation = Quaternion.Euler(leftHandHolder.eulerAngles + _leftHandExtraRotation);
            _rightHandRigController.rotation = Quaternion.Euler(rightHandHolder.eulerAngles + _rightHandExtraRotation);
        }

        public void Reload()
        {
            _bulletPool?.ReturnToPoolEverything();
        }

        protected Pool _bulletPool;
        protected float _spread;

        protected PlayerBulletBehavior SpawnBullet(int id, bool init = true)
        {
            var angle = GetAngle(id);
            var settings = PoolSettings(angle);
            var bullet = _bulletPool.GetPlayerBullet(settings);
            bullet.owner = Owner;
            if (init) bullet.Initialise(Damage, BulletSpeed, Target, bulletLifeTime);
            return bullet;
        }

        public void Unload()
        {
            if (_bulletPool == null) return;
            _bulletPool.Clear();
            _bulletPool = null;
        }

        public abstract void PlaceGun(BaseCharacterGraphics characterGraphics);
        public abstract void RecalculateDamage();
        public AnimationClip GetShootAnimationClip() => characterShootAnimation;

        public virtual void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void SetDamage(DuoInt damage)
        {
            this.damage = damage;
        }

        public void SetDamage(int minDamage, int maxDamage)
        {
            damage = new DuoInt(minDamage, maxDamage);
        }

        public void PlayUpgradeParticle()
        {
            var particleCase = ParticlesController.Play(_particleUpgrade).SetPosition(transform.position + upgradeParticleOffset).SetScale(upgradeParticleSize.ToVector3());
            particleCase.ParticleSystem.transform.rotation = CameraController.MainCamera.transform.rotation;
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + upgradeParticleOffset, upgradeParticleSize.ToVector3());
        }

#if UNITY_EDITOR
        [Button("Prepare Weapon")]
        void PrepareWeapon()
        {
            if (leftHandHolder == null)
            {
                var leftHandHolderObject = new GameObject("Left Hand Holder");
                leftHandHolderObject.transform.SetParent(transform);
                leftHandHolderObject.transform.ResetLocal();
                leftHandHolderObject.transform.localPosition = new Vector3(-0.4f, 0, 0);

                var iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_3");
                UnityEditor.EditorGUIUtility.SetIconForObject(leftHandHolderObject, (Texture2D)iconContent.image);

                leftHandHolder = leftHandHolderObject.transform;
            }

            if (rightHandHolder == null)
            {
                var rightHandHolderObject = new GameObject("Right Hand Holder");
                rightHandHolderObject.transform.SetParent(transform);
                rightHandHolderObject.transform.ResetLocal();
                rightHandHolderObject.transform.localPosition = new Vector3(0.4f, 0, 0);

                var iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_4");
                UnityEditor.EditorGUIUtility.SetIconForObject(rightHandHolderObject, (Texture2D)iconContent.image);

                rightHandHolder = rightHandHolderObject.transform;
            }

            if (shootPoint == null)
            {
                var shootingPointObject = new GameObject("Shooting Point");
                shootingPointObject.transform.SetParent(transform);
                shootingPointObject.transform.ResetLocal();
                shootingPointObject.transform.localPosition = new Vector3(0, 0, 1);

                var iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_1");
                UnityEditor.EditorGUIUtility.SetIconForObject(shootingPointObject, (Texture2D)iconContent.image);

                shootPoint = shootingPointObject.transform;
            }

            if (characterShootAnimation == null)
            {
                characterShootAnimation = RuntimeEditorUtils.GetAssetByName<AnimationClip>("Shot");
            }
        }
#endif
    }
}