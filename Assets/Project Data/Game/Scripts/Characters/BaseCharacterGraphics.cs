using UnityEngine;
using UnityEngine.Animations.Rigging;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public abstract class BaseCharacterGraphics : MonoBehaviour
    {
        static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Upgrade");

        readonly int _animationShotHash = Animator.StringToHash("Shot");
        readonly int _animationHitHash = Animator.StringToHash("Hit");

        readonly int _jumpAnimationHash = Animator.StringToHash("Jump");
        readonly int _gruntAnimationHash = Animator.StringToHash("Grunt");

        [SerializeField]
        protected Animator characterAnimator;

        [Space]
        [SerializeField] SkinnedMeshRenderer meshRenderer;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;

        [Header("Movement")]
        [SerializeField] MovementSettings movementSettings;
        public MovementSettings MovementSettings => movementSettings;

        [SerializeField] MovementSettings movementAimingSettings;
        public MovementSettings MovementAimingSettings => movementAimingSettings;

        [Header("Hands Rig")]
        [SerializeField] TwoBoneIKConstraint leftHandRig;
        public TwoBoneIKConstraint LeftHandRig => leftHandRig;

        [SerializeField] Vector3 leftHandExtraRotation;
        public Vector3 LeftHandExtraRotation => leftHandExtraRotation;

        [SerializeField] TwoBoneIKConstraint rightHandRig;
        public TwoBoneIKConstraint RightHandRig => rightHandRig;

        [SerializeField] Vector3 rightHandExtraRotation;
        public Vector3 RightHandExtraRotation => rightHandExtraRotation;

        [Header("Weapon")]
        [SerializeField] Transform weaponsTransform;

        [SerializeField] Transform minigunHolderTransform;
        public Transform MinigunHolderTransform => minigunHolderTransform;

        [SerializeField] Transform shootGunHolderTransform;
        public Transform ShootGunHolderTransform => shootGunHolderTransform;

        [SerializeField] Transform rocketHolderTransform;
        public Transform RocketHolderTransform => rocketHolderTransform;

        [SerializeField] Transform teslaHolderTransform;
        public Transform TeslaHolderTransform => teslaHolderTransform;

        [Space]
        [SerializeField] Rig mainRig;
        [SerializeField] Transform leftHandController;
        [SerializeField] Transform rightHandController;

        protected CharacterBehaviour CharacterBehaviour;
        protected CharacterAnimationHandler AnimationHandler;

        public Material CharacterMaterial;
 

        int _animatorShootingLayerIndex;

        AnimatorOverrideController _animatorOverrideController;

        TweenCase _rigWeightCase;

        protected RagdollBehavior Ragdoll;

        public virtual void Initialise(CharacterBehaviour characterBehaviour)
        {
            this.CharacterBehaviour = characterBehaviour;

            Ragdoll = new RagdollBehavior();
            Ragdoll.Initialise(characterAnimator.transform);

            AnimationHandler = characterAnimator.GetComponent<CharacterAnimationHandler>();
            AnimationHandler.Inititalise(characterBehaviour);

            _animatorOverrideController = new AnimatorOverrideController(characterAnimator.runtimeAnimatorController);
            characterAnimator.runtimeAnimatorController = _animatorOverrideController;

            CharacterMaterial = meshRenderer.sharedMaterial;

            _animatorShootingLayerIndex = characterAnimator.GetLayerIndex("Shooting");
        }

        public abstract void OnMovingStarted();
        public abstract void OnMovingStoped();
        public abstract void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound);

        public virtual void OnDeath() { }

        public void Jump()
        {
            characterAnimator.SetTrigger(_jumpAnimationHash);

            _rigWeightCase.KillActive();
            mainRig.weight = 0f;
        }

        public void Grunt()
        {
            characterAnimator.SetTrigger(_gruntAnimationHash);

            var strength = 0.1f;
            var durationIn = 0.1f;
            var durationOut = 0.15f;

            weaponsTransform.DOMoveY(weaponsTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                weaponsTransform.DOMoveY(weaponsTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            leftHandController.DOMoveY(leftHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                leftHandController.DOMoveY(leftHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            rightHandController.DOMoveY(rightHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                rightHandController.DOMoveY(rightHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });
        }

        public void EnableRig()
        {
            _rigWeightCase = Tween.DoFloat(0, 1, 0.2f, (value) => mainRig.weight = value);
        }

        public abstract void CustomFixedUpdate();

        public void SetShootingAnimation(AnimationClip animationClip)
        {
            _animatorOverrideController["Shot"] = animationClip;
        }

        public void OnShoot()
        {
            characterAnimator.Play(_animationShotHash, _animatorShootingLayerIndex, 0);
        }

        public void PlayHitAnimation()
        {
            characterAnimator.SetTrigger(_animationHitHash);
        }

        public void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void PlayUpgradeParticle()
        {
            var particleCase = ParticlesController.Play(PARTICLE_UPGRADE).SetPosition(transform.position + new Vector3(0, 0.5f, 0)).SetScale((5).ToVector3());
            particleCase.ParticleSystem.transform.rotation = CameraController.MainCamera.transform.rotation;
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        public void EnableRagdoll()
        {
            mainRig.weight = 0.0f;

            characterAnimator.enabled = false;

            CharacterBehaviour.Weapon.gameObject.SetActive(false);

            Ragdoll?.ActivateWithForce(transform.position + transform.forward, 700, 100);
        }

        public void DisableRagdoll()
        {
            Ragdoll?.Disable();

            mainRig.weight = 1.0f;

            CharacterBehaviour.Weapon.gameObject.SetActive(true);
            characterAnimator.enabled = true;
        }

        public abstract void Unload();

        public abstract void Reload();

        public abstract void Disable();

        public abstract void Activate();

#if UNITY_EDITOR
        [Button("Prepare Model")]
        public void PrepareModel()
        {
            // Get animator component
            var tempAnimator = characterAnimator;

            if (tempAnimator != null)
            {
                if (tempAnimator.avatar != null && tempAnimator.avatar.isHuman)
                {
                    // Initialise rig
                    var rigBuilder = tempAnimator.GetComponent<RigBuilder>();
                    if (rigBuilder == null)
                    {
                        rigBuilder = tempAnimator.gameObject.AddComponent<RigBuilder>();

                        var rigObject = new GameObject("Main Rig");
                        rigObject.transform.SetParent(tempAnimator.transform);
                        rigObject.transform.ResetLocal();

                        var rig = rigObject.AddComponent<Rig>();

                        mainRig = rig;

                        rigBuilder.layers.Add(new RigLayer(rig, true));

                        // Left hand rig
                        var leftHandRigObject = new GameObject("Left Hand Rig");
                        leftHandRigObject.transform.SetParent(rigObject.transform);
                        leftHandRigObject.transform.ResetLocal();

                        var leftHandControllerObject = new GameObject("Controller");
                        leftHandControllerObject.transform.SetParent(leftHandRigObject.transform);
                        leftHandControllerObject.transform.ResetLocal();

                        leftHandController = leftHandControllerObject.transform;

                        var leftHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                        leftHandControllerObject.transform.position = leftHandBone.position;
                        leftHandControllerObject.transform.rotation = leftHandBone.rotation;

                        var leftHandRig = leftHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        leftHandRig.data.target = leftHandControllerObject.transform;
                        leftHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                        leftHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        leftHandRig.data.tip = leftHandBone;

                        // Right hand rig
                        var rightHandRigObject = new GameObject("Right Hand Rig");
                        rightHandRigObject.transform.SetParent(rigObject.transform);
                        rightHandRigObject.transform.ResetLocal();

                        var rightHandControllerObject = new GameObject("Controller");
                        rightHandControllerObject.transform.SetParent(rightHandRigObject.transform);
                        rightHandControllerObject.transform.ResetLocal();

                        rightHandController = rightHandControllerObject.transform;

                        var rightHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.RightHand);
                        rightHandControllerObject.transform.position = rightHandBone.position;
                        rightHandControllerObject.transform.rotation = rightHandBone.rotation;

                        var rightHandRig = rightHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        rightHandRig.data.target = rightHandControllerObject.transform;
                        rightHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                        rightHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        rightHandRig.data.tip = rightHandBone;

                        this.leftHandRig = leftHandRig;
                        this.rightHandRig = rightHandRig;
                    }

                    // Prepare ragdoll
                    RagdollHelper.CreateRagdoll(tempAnimator, 60, 1, LayerMask.NameToLayer("Ragdoll"));

                    movementSettings.rotationSpeed = 8;
                    movementSettings.moveSpeed = 5;
                    movementSettings.acceleration = 781.25f;
                    movementSettings.animationMultiplier = new DuoFloat(0, 1.4f);

                    movementAimingSettings.rotationSpeed = 8;
                    movementAimingSettings.moveSpeed = 4.375f;
                    movementAimingSettings.acceleration = 781.25f;
                    movementAimingSettings.animationMultiplier = new DuoFloat(0, 1.2f);

                    var tempAnimationHandler = tempAnimator.GetComponent<CharacterAnimationHandler>();
                    if (tempAnimationHandler == null)
                        tempAnimator.gameObject.AddComponent<CharacterAnimationHandler>();

                    // Create weapon holders
                    var weaponHolderObject = new GameObject("Weapons");
                    weaponHolderObject.transform.SetParent(tempAnimator.transform);
                    weaponHolderObject.transform.ResetLocal();

                    weaponsTransform = weaponHolderObject.transform;

                    // Minigun
                    var miniGunHolderObject = new GameObject("Minigun Holder");
                    miniGunHolderObject.transform.SetParent(weaponsTransform);
                    miniGunHolderObject.transform.ResetLocal();
                    miniGunHolderObject.transform.localPosition = new Vector3(0.204f, 0.7f, 0.375f);

                    minigunHolderTransform = miniGunHolderObject.transform;

                    // Shotgun
                    var shotgunHolderObject = new GameObject("Shotgun Holder");
                    shotgunHolderObject.transform.SetParent(weaponsTransform);
                    shotgunHolderObject.transform.ResetLocal();
                    shotgunHolderObject.transform.localPosition = new Vector3(0.22f, 0.6735f, 0.23f);

                    shootGunHolderTransform = shotgunHolderObject.transform;

                    // Rocket
                    var rocketHolderObject = new GameObject("Rocket Holder");
                    rocketHolderObject.transform.SetParent(weaponsTransform);
                    rocketHolderObject.transform.ResetLocal();
                    rocketHolderObject.transform.localPosition = new Vector3(0.234f, 0.726f, 0.369f);
                    rocketHolderObject.transform.localRotation = Quaternion.Euler(-23.68f, -4.74f, 7.92f);

                    rocketHolderTransform = rocketHolderObject.transform;

                    // Tesla
                    var teslaHolderObject = new GameObject("Tesla Holder");
                    teslaHolderObject.transform.SetParent(weaponsTransform);
                    teslaHolderObject.transform.ResetLocal();
                    teslaHolderObject.transform.localPosition = new Vector3(0.213f, 0.783f, 0.357f);

                    teslaHolderTransform = teslaHolderObject.transform;

                    // Initialise mesh renderer
                    meshRenderer = tempAnimator.transform.GetComponentInChildren<SkinnedMeshRenderer>();

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                else
                {
                    Debug.LogError("Avatar is missing or type isn't humanoid!");
                }
            }
            else
            {
                Debug.LogWarning("Animator component can't be found!");
            }
        }
#endif
    }
}