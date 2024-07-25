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
        protected CharacterBehaviour CharacterBehaviour;
        [SerializeField] protected List<float> bulletStreamAngles = new() { 0 };
        protected CharacterBehaviour Owner => CharacterBehaviour;
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
        protected void TargetUnreachable() => CharacterBehaviour.TargetUnreachable();
        protected float BulletSpeed => _bulletSpeed.Random();
        protected BaseEnemyBehavior Target => CharacterBehaviour.ClosestEnemyBehaviour;
        protected float _nextShootTime;
        protected float _attackDelay;
        protected Vector3 _shootDirection;
        [SerializeField] protected LayerMask targetLayers;
        protected bool OutOfAngle => !(Vector3.Angle(_shootDirection, transform.forward.SetY(0f)) < 40f);

        protected float FireRate()
        {
            return Time.timeSinceLevelLoad + _attackDelay / CharacterBehaviour.AtkSpdMult;
        }

        protected bool TargetInSight =>
           // Physics.Raycast(transform.position, _shootDirection, out var hit, 300f, targetLayers)
            Physics.Raycast(transform.position - _shootDirection.normalized, _shootDirection, out var hit, 300f, targetLayers)
            && hit.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY;

        protected Vector3 AimAtTarget()
        {
            return CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
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

        protected int RandomBulletsAmount(BaseWeaponUpgrade upgrade)
        {
            return upgrade.GetCurrentStage().BulletsPerShot.Random() + CharacterBehaviour.MultishotBoosterAmount;
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

        public virtual void GunUpdate()
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

        public abstract void Reload();
        public abstract void OnGunUnloaded();
        protected Pool _bulletPool;

        public void Unload()
        {
            if (_bulletPool == null) return;
            _bulletPool.Clear();
            _bulletPool = null;
            OnGunUnloaded();
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
            var particleCase = ParticlesController.Play(_particleUpgrade)
                .SetPosition(transform.position + upgradeParticleOffset).SetScale(upgradeParticleSize.ToVector3());
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