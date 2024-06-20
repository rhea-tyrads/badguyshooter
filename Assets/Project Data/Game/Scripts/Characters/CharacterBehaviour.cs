using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;
using Random = UnityEngine.Random;

namespace Watermelon.SquadShooter
{
    public class CharacterBehaviour : MonoBehaviour, IEnemyDetector, IHealth, INavMeshAgent
    {
        #region Inspector

        public GameObject moveSpeedBoostVfx;
        public GameObject atkSpeedBoostVfx;
        public GameObject multishotBoostVfx;
        public float critMultiplier = 2f;
        public float critChance = 0.15f;
        public bool IsCritical;
        public int respawnCount;
        public float CritMult => IsCritical && Random.value <= critChance ? critMultiplier : 1f;
        static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");
        static CharacterBehaviour characterBehaviour;

        [SerializeField] NavMeshAgent agent;
        [SerializeField] EnemyDetector enemyDetector;

        [Header("Health")]
        [SerializeField] HealthbarBehaviour healthbarBehaviour;
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        [SerializeField] ParticleSystem healingParticle;

        [Header("Target")]
        [SerializeField] GameObject targetRingPrefab;
        [SerializeField] Color targetRingActiveColor;
        [SerializeField] Color targetRingDisabledColor;
        [SerializeField] Color targetRingSpecialColor;

        [Space(5)]
        [SerializeField] AimRingBehavior aimRingBehavior;
        [SerializeField] ParticleSystem stunVfx;

        public bool isMoveSpeedBooster;
        public bool isMultishotBooster;
        public float multishotBoosterDuration = 10;
        public float moveSpeedBoostDuration = 10;
        public int MultishotBoosterAmount => isMultishotBooster ? 4 : 0;
        float multiShotTimer;
        float moveSpeedBoostTimer;

        public bool isAtkSpdBooster;
        public float atkSpdBoosterDuration = 10;
        public float atkSpdBoosterMult = 2;

        float atkSpdTimer;

        // Character Graphics

        BaseCharacterGraphics graphics;
        public BaseCharacterGraphics Graphics => graphics;

        GameObject graphicsPrefab;
        SkinnedMeshRenderer characterMeshRenderer;

        MaterialPropertyBlock hitShinePropertyBlock;
        TweenCase hitShineTweenCase;

        public CharacterStats stats;
        public CharacterStats Stats => stats;

        // Gun
        BaseGunBehavior gunBehaviour;
        public BaseGunBehavior Weapon => gunBehaviour;

        GameObject gunPrefabGraphics;

        // Health
        float currentHealth;
        float hpBonusMultiplier = 1.3f;
        public bool isHpBonus;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health * HpBonus;
        float HpBonus => isHpBonus ? hpBonusMultiplier : 1f;
        public bool FullHealth => currentHealth == stats.Health;

        public bool IsActive => isActive;
        bool isActive;

        public static Transform Transform => characterBehaviour.transform;

        // Movement
        MovementSettings movementSettings;
        MovementSettings movementAimingSettings;

        MovementSettings activeMovementSettings;
        public MovementSettings MovementSettings => activeMovementSettings;

        bool isMoving;
        float speed = 0;

        Vector3 movementVelocity;
        public Vector3 MovementVelocity => movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        public bool IsCloseEnemyFound => closestEnemyBehaviour != null;

        BaseEnemyBehavior closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;

        Transform playerTarget;
        GameObject targetRing;
        Renderer targetRingRenderer;
        TweenCase ringTweenCase;

        VirtualCameraCase mainCameraCase;
        public VirtualCameraCase MainCameraCase => mainCameraCase;

        bool isMovementActive = false;
        public bool IsMovementActive => isMovementActive;

        public static bool NoDamage { get; private set; } = false;

        public static bool IsDead { get; private set; } = false;

        public static SimpleCallback OnDied;

        #endregion

        float stunTimer;
        public bool isStunned;

        void Awake()
        {
            moveSlowFactor = 1;
            agent.enabled = false;
        }

        public void Initialise()
        {
            if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((false));
            if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive((false));
            if (multishotBoostVfx) multishotBoostVfx.SetActive((false));
            characterBehaviour = this;
            hitShinePropertyBlock = new MaterialPropertyBlock();
            isActive = false;
            enabled = false;

            var tempTarget = new GameObject("[TARGET]")
            {
                transform = {position = transform.position}
            };
            tempTarget.SetActive(true);
            playerTarget = tempTarget.transform;

            mainCameraCase = CameraController.GetCamera(CameraType.Main);
            enemyDetector.Initialise(this);
            currentHealth = MaxHealth;
            healthbarBehaviour.Initialise(transform, this, true,
                CharactersController.SelectedCharacter.GetCurrentStage().HealthBarOffset);
            aimRingBehavior.Init(transform);
            targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity);
            targetRingRenderer = targetRing.GetComponent<Renderer>();
            aimRingBehavior.Hide();
            IsDead = false;
            stunVfx.gameObject.SetActive(false);
        }

        public void ApplyCriticalBonus()
        {
            IsCritical = true;
        }

        public void ApplyHitpointsBonus()
        {
            isHpBonus = true;
            currentHealth = MaxHealth;
        }

        public void ApplyRespawnBonus()
        {
            respawnCount = 1;
        }

        public bool RespawnInUse;

        public void UseRespawn()
        {
            Debug.LogError("RESPAWNED");
            RespawnInUse = true;
            respawnCount--;
            Invoke(nameof(ReviveMe), 1f);
            // Control.DisableMovementControl();
            // LevelController.OnPlayerDiedCall();
            // GameController.OnRevive();

            //  Reload();
        }

        void ReviveMe()
        {
            GameController.OnRevive();
        }

        public void Reload(bool resetHealth = true)
        {
            if (resetHealth)
                currentHealth = MaxHealth;

            isHpBonus = false;
            IsCritical = false;
            IsDead = false;
            stunTimer = 0;
            atkSpdTimer = 0;
            moveSlowTimer = 0;
            multiShotTimer = 0f;

            if (multishotBoostVfx) multishotBoostVfx.SetActive((false));
            if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive((false));
            if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((false));

            healthbarBehaviour.EnableBar();
            healthbarBehaviour.RedrawHealth();
            enemyDetector.Reload();
            enemyDetector.gameObject.SetActive(false);
            graphics.DisableRagdoll();
            graphics.Reload();
            gunBehaviour.Reload();
            gameObject.SetActive(true);
        }

        public void ResetDetector()
        {
            var radius = enemyDetector.DetectorRadius;
            enemyDetector.SetRadius(0);
            Tween.NextFrame(() => enemyDetector.SetRadius(radius), framesOffset: 2,
                updateMethod: UpdateMethod.FixedUpdate);
        }

        public void Unload()
        {
            if (graphics != null)
                graphics.Unload();

            if (playerTarget != null)
                Destroy(playerTarget.gameObject);

            if (aimRingBehavior != null)
                Destroy(aimRingBehavior.gameObject);

            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy();
        }

        public void OnLevelLoaded()
        {
            if (gunBehaviour != null)
                gunBehaviour.OnLevelLoaded();
        }

        public void OnNavMeshUpdated()
        {
            if (agent.isOnNavMesh)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }

        public void ActivateAgent()
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        public static void DisableNavmeshAgent()
        {
            characterBehaviour.agent.enabled = false;
        }

        public float moveSlowFactor;
        public float moveBoostFactor = 1;
        bool moveSlowed;
        const float moveSlowDuration = 3f;
        public float moveSlowTimer;

        public void ApplyMovementSlow(float factor)
        {
            moveSlowFactor = factor;
            moveSlowed = true;
            moveSlowTimer = moveSlowDuration;
        }

        public virtual void TakeDamage(float damage)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);
            healthbarBehaviour.OnHealthChanged();
            mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            if (currentHealth <= 0)
            {
                healthbarBehaviour.DisableBar();
                OnCloseEnemyChanged(null);

                isActive = false;
                enabled = false;
                enemyDetector.gameObject.SetActive(false);
                aimRingBehavior.Hide();

                OnDeath();

                graphics.EnableRagdoll();
                OnDied?.Invoke();
                Vibration.Vibrate(VibrationIntensity.Medium);
            }

            HitEffect();

            AudioController.PlaySound(AudioController.Sounds.characterHit.GetRandomItem());

            Vibration.Vibrate(VibrationIntensity.Light);

            FloatingTextController.SpawnFloatingText("PlayerHit", "-" + damage.ToString("F0"),
                transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 3.75f, Random.Range(-0.1f, 0.1f)),
                Quaternion.identity, 1f);
        }

        [Button]
        public void OnDeath()
        {
            graphics.OnDeath();
            IsDead = true;
            if (respawnCount > 0)
            {
                UseRespawn();
                return;
            }

            Tween.DelayedCall(0.5f, LevelController.OnPlayerDied);
        }

        public void SetPosition(Vector3 position)
        {
            playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        protected void HitEffect()
        {
            hitShineTweenCase.KillActive();

            characterMeshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white);
            characterMeshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            hitShineTweenCase = characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH,
                hitShinePropertyBlock, Color.black, 0.32f);

            graphics.PlayHitAnimation();
        }

        #region Gun

        public void SetGun(WeaponData weaponData, bool playBounceAnimation = false, bool playAnimation = false,
            bool playParticle = false)
        {
            var gunUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
            var currentStage = gunUpgrade.GetCurrentStage();

            // Check if graphics isn't exist already
            if (gunPrefabGraphics != gunUpgrade.WeaponPrefab)
            {
                // Store prefab link
                gunPrefabGraphics = gunUpgrade.WeaponPrefab;

                if (gunBehaviour)
                {
                    gunBehaviour.OnGunUnloaded();

                    Destroy(gunBehaviour.gameObject);
                }

                if (gunPrefabGraphics)
                {
                    var gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);

                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();

                    if (graphics)
                    {
                        gunBehaviour.InitialiseCharacter(graphics);
                        gunBehaviour.PlaceGun(graphics);

                        graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                        gunBehaviour.UpdateHandRig();
                    }
                }
            }

            if (gunBehaviour)
            {
                gunBehaviour.Initialise(this, weaponData);

                var defaultScale = gunBehaviour.transform.localScale;

                if (playAnimation)
                {
                    gunBehaviour.transform.localScale = defaultScale * 0.8f;
                    gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut);
                }

                if (playBounceAnimation)
                    gunBehaviour.PlayBounceAnimation();

                if (playParticle)
                    gunBehaviour.PlayUpgradeParticle();
            }

            enemyDetector.SetRadius(currentStage.RangeRadius);
            aimRingBehavior.SetRadius(currentStage.RangeRadius);
        }

        public void OnGunShooted()
        {
            graphics.OnShoot();
        }

        #endregion

        #region Graphics

        public void SetStats(CharacterStats stats)
        {
            this.stats = stats;

            currentHealth = stats.Health;

            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();
        }

        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            if (graphicsPrefab == newGraphicsPrefab) return;
            graphicsPrefab = newGraphicsPrefab;

            if (graphics != null)
            {
                if (gunBehaviour != null)
                    gunBehaviour.transform.SetParent(null);

                graphics.Unload();

                Destroy(graphics.gameObject);
            }

            var graphicObject = Instantiate(newGraphicsPrefab, transform, true);
            graphicObject.transform.ResetLocal();
            graphicObject.SetActive(true);
            graphics = graphicObject.GetComponent<BaseCharacterGraphics>();
            graphics.Initialise(this);
            movementSettings = graphics.MovementSettings;
            movementAimingSettings = graphics.MovementAimingSettings;
            activeMovementSettings = movementSettings;
            characterMeshRenderer = graphics.MeshRenderer;

            if (gunBehaviour != null)
            {
                gunBehaviour.InitialiseCharacter(graphics);
                gunBehaviour.PlaceGun(graphics);
                graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());
                gunBehaviour.UpdateHandRig();
                Jump();
            }
            else
            {
                Tween.NextFrame(Jump, 0, false, UpdateMethod.LateUpdate);
            }

            if (playParticle)
                graphics.PlayUpgradeParticle();

            if (playAnimation)
                graphics.PlayBounceAnimation();
        }

        #endregion

        public void Activate(bool check = true)
        {
            if (check && isActive) return;
            isActive = true;

            enabled = true;
            enemyDetector.gameObject.SetActive(true);
            aimRingBehavior.Show();
            graphics.Activate();
            NavMeshController.InvokeOrSubscribe(this);
        }

        public void Disable()
        {
            if (!isActive) return;
            isActive = false;

            enabled = false;
            agent.enabled = false;
            aimRingBehavior.Hide();
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);
            graphics.Disable();
            closestEnemyBehaviour = null;

            if (!isMoving) return;
            isMoving = false;
            speed = 0;
        }

        public void MoveForwardAndDisable(float duration)
        {
            agent.enabled = false;
            transform.DOMove(transform.position + Vector3.forward * (activeMovementSettings.MoveSpeed * duration),
                duration).OnComplete(() => { Disable(); });
        }

        public void DisableAgent()
        {
            agent.enabled = false;
        }

        public void ActivateMovement()
        {
            isMovementActive = true;
            aimRingBehavior.Show();
        }

        void Update()
        {
            if (moveSlowed)
            {
                if (moveSlowTimer <= 0)
                {
                    moveSlowFactor = 1f;
                    moveSlowed = false;
                }

                moveSlowTimer -= Time.deltaTime;
            }

            if (isMoveSpeedBooster)
            {
                if (moveSpeedBoostTimer <= 0)
                {
                    moveBoostFactor = 1f;
                    isMoveSpeedBooster = false;
                    if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((false));
                }

                moveSpeedBoostTimer -= Time.deltaTime;
            }

            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (!(stunTimer <= 0)) return;
                isStunned = false;
                stunVfx.gameObject.SetActive(false);

                return;
            }

            if (gunBehaviour)
                gunBehaviour.UpdateHandRig();

            if (!isActive) return;

            var joystick = Control.CurrentControl;

            if (joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.1f)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    speed = 0;
                    graphics.OnMovingStarted();
                }

                var maxAlowedSpeed = Mathf.Clamp01(joystick.MovementInput.magnitude) *
                                     activeMovementSettings.MoveSpeed * moveSlowFactor * moveBoostFactor;

                if (speed > maxAlowedSpeed)
                {
                    speed -= activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed < maxAlowedSpeed)
                        speed = maxAlowedSpeed;
                }
                else
                {
                    speed += activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed > maxAlowedSpeed)
                        speed = maxAlowedSpeed;
                }

                movementVelocity = transform.forward * speed;

                transform.position += joystick.MovementInput * (Time.deltaTime * speed);

                graphics.OnMoving(Mathf.InverseLerp(0, activeMovementSettings.MoveSpeed, speed), joystick.MovementInput,
                    IsCloseEnemyFound);

                if (!IsCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(joystick.MovementInput.normalized),
                        Time.deltaTime * activeMovementSettings.RotationSpeed);
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    movementVelocity = Vector3.zero;
                    graphics.OnMovingStoped();
                    speed = 0;
                }
            }

            if (IsCloseEnemyFound)
            {
                playerTarget.position = Vector3.Lerp(playerTarget.position,
                    new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y,
                        closestEnemyBehaviour.transform.position.z),
                    Time.deltaTime * activeMovementSettings.RotationSpeed);

                transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
            }

            targetRing.transform.rotation = Quaternion.identity;

            if (healthbarBehaviour)
                healthbarBehaviour.FollowUpdate();

            aimRingBehavior.UpdatePosition();
        }

        void FixedUpdate()
        {
            if (isStunned) return;
            graphics.CustomFixedUpdate();

            if (gunBehaviour)
                gunBehaviour.GunUpdate();

            if (isMultishotBooster)
            {
                multiShotTimer -= Time.fixedDeltaTime;
                if (multiShotTimer <= 0)
                {
                    MultishotOff();
                }
            }

            if (isAtkSpdBooster)
            {
                atkSpdTimer -= Time.fixedDeltaTime;
                if (atkSpdTimer <= 0)
                {
                    isAtkSpdBooster = false;
                    if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive(false);
                }
            }
        }

        public void MultishotOff()
        {
            isMultishotBooster = false;
            if (multishotBoostVfx) multishotBoostVfx.SetActive((false));
        }

        public float AtkSpdMult => isAtkSpdBooster ? atkSpdBoosterMult : 1f;

        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (!isActive) return;
//if(enemyBehavior is MeleeEnemyBehaviour {isInStealth: true})return;

            if (enemyBehavior)
            {
                if (!closestEnemyBehaviour)
                    playerTarget.position = transform.position + transform.forward * 5;
                activeMovementSettings = movementAimingSettings;
                closestEnemyBehaviour = enemyBehavior;
                targetRing.SetActive(true);
                targetRing.transform.rotation = Quaternion.identity;
                ringTweenCase.KillActive();
                targetRing.transform.SetParent(enemyBehavior.transform);
                targetRing.transform.localScale = Vector3.one * (enemyBehavior.Stats.TargetRingSize * 1.4f);
                targetRing.transform.localPosition = Vector3.zero;
                ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f)
                    .SetEasing(Ease.Type.BackIn);
                CameraController.SetEnemyTarget(enemyBehavior);
                SetTargetActive();
                return;
            }

            activeMovementSettings = movementSettings;
            closestEnemyBehaviour = null;
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);
            CameraController.SetEnemyTarget(null);
        }

        public static BaseEnemyBehavior GetClosestEnemy() => characterBehaviour.enemyDetector.ClosestEnemy;

        public static CharacterBehaviour GetBehaviour() => characterBehaviour;

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        public void SetTargetActive()
        {
            targetRingRenderer.material.color = closestEnemyBehaviour && closestEnemyBehaviour.Tier == EnemyTier.Elite
                ? targetRingSpecialColor
                : targetRingActiveColor;
        }

        public void SetTargetUnreachable()
        {
            targetRingRenderer.material.color = targetRingDisabledColor;
        }

        public float chestOpenDistance = 3f;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                var item = other.GetComponent<IDropableItem>();
                if (!item.IsPickable(this)) return;
                OnItemPicked(item);
                item.Pick();
            }
            else if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                chest = other.GetComponent<AbstractChestBehavior>();

                var dist = Vector3.Distance(transform.position, other.transform.position);
                //   Debug.LogError(other.transform.name + " dist " + dist, other.transform);
                //   Debug.LogError(other.transform.name,other.transform);
                if (dist < chestOpenDistance)
                    chest.ChestApproached();
            }
        }

        AbstractChestBehavior chest;
        bool isGood;

        void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(PhysicsHelper.TAG_CHEST)) return;
            if (!chest) return;
            if (chest.Appriached) return;
            var dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < chestOpenDistance)
                chest.ChestApproached();
        }

        public void ApplyStun(float duration)
        {
            Debug.LogError("Stunned");
            isStunned = true;
            stunTimer = duration;
            stunVfx.gameObject.SetActive(true);
            StartCoroutine(PlayHit());
        }

        IEnumerator PlayHit()
        {
            while (stunTimer > 0)
            {
                graphics.PlayHitAnimation();
                yield return new WaitForSeconds(0.3f);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestLeft();
                chest = null;
            }
        }

        public void OnItemPicked(IDropableItem item)
        {
            switch (item.DropType)
            {
                case DropableItemType.Currency when item.DropData.currencyType == CurrencyType.Coins:
                {
                    if (item.IsRewarded)
                        LevelController.OnRewardedCoinPicked(item.DropAmount);
                    else
                        LevelController.OnCoinPicked(item.DropAmount);
                    break;
                }
                case DropableItemType.Currency:
                    CurrenciesController.Add(item.DropData.currencyType, item.DropAmount);
                    break;
                case DropableItemType.Heal:
                    currentHealth = Mathf.Clamp(currentHealth + item.DropAmount, 0, MaxHealth);
                    healthbarBehaviour.OnHealthChanged();
                    healingParticle.Play();
                    break;
                case DropableItemType.AtkSpeedBooster:
                    FireRateBonus();
                    break;
                case DropableItemType.MultishotBooster:
                    MultishotBonus();
                    break;
                case DropableItemType.MoveSpeedBooster:
                    MovementBonus();
                    break;
            }
        }

        public void FireRateBonus(bool forever = false)
        {
            isAtkSpdBooster = true;
            atkSpdTimer = forever ? 99999999999999f : atkSpdBoosterDuration;
            if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive(true);
        }

        public void MovementBonus(bool forever = false)
        {
            isMoveSpeedBooster = true;
            moveSpeedBoostTimer = forever ? 9999999999999999f : moveSpeedBoostDuration;
            moveBoostFactor = 1.3f;
            if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((true));
        }

        public void MultishotBonus(bool forever = false)
        {
            isMultishotBooster = true;
            multiShotTimer = forever ? 999999999999f : multishotBoosterDuration;
            if (multishotBoostVfx) multishotBoostVfx.SetActive((true));
        }


        [Button]
        public void Jump()
        {
            graphics.Jump();
            gunBehaviour.transform.localScale = Vector3.zero;
            gunBehaviour.gameObject.SetActive(false);
        }

        public void SpawnWeapon()
        {
            graphics.EnableRig();
            gunBehaviour.gameObject.SetActive(true);
            gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
        }

        void OnDestroy()
        {
            if (healthbarBehaviour.HealthBarTransform != null)
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);

            if (aimRingBehavior != null)
                aimRingBehavior.OnPlayerDestroyed();
        }
    }
}