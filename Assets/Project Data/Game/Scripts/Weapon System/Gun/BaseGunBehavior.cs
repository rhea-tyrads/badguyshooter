using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon.SquadShooter
{
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Gun Upgrade");

        [Header("Animations")]
        [SerializeField] AnimationClip characterShootAnimation;

        [Space]
        [SerializeField] Transform leftHandHolder;
        [SerializeField] Transform rightHandHolder;

        [Space]
        [SerializeField] protected Transform shootPoint;

        [Header("Upgrade")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [SerializeField] float upgradeParticleSize = 1.0f;

        protected CharacterBehaviour CharacterBehaviour;
        public CharacterBehaviour Owner => CharacterBehaviour;
        protected WeaponData Data;

         public DuoInt damage;
        Transform _leftHandRigController;
        Vector3 _leftHandExtraRotation;
        Transform _rightHandRigController;
        Vector3 _rightHandExtraRotation;

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
            var particleCase = ParticlesController.Play(PARTICLE_UPGRADE)
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
                UnityEditor.EditorGUIUtility.SetIconForObject(leftHandHolderObject, (Texture2D) iconContent.image);

                leftHandHolder = leftHandHolderObject.transform;
            }

            if (rightHandHolder == null)
            {
                var rightHandHolderObject = new GameObject("Right Hand Holder");
                rightHandHolderObject.transform.SetParent(transform);
                rightHandHolderObject.transform.ResetLocal();
                rightHandHolderObject.transform.localPosition = new Vector3(0.4f, 0, 0);

                var iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_4");
                UnityEditor.EditorGUIUtility.SetIconForObject(rightHandHolderObject, (Texture2D) iconContent.image);

                rightHandHolder = rightHandHolderObject.transform;
            }

            if (shootPoint == null)
            {
                var shootingPointObject = new GameObject("Shooting Point");
                shootingPointObject.transform.SetParent(transform);
                shootingPointObject.transform.ResetLocal();
                shootingPointObject.transform.localPosition = new Vector3(0, 0, 1);

                var iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_1");
                UnityEditor.EditorGUIUtility.SetIconForObject(shootingPointObject, (Texture2D) iconContent.image);

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