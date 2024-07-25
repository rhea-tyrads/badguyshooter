using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("IsCritical")]
        public bool isCritical;
        public int respawnCount;
        public float CritMult => isCritical && Random.value <= critChance ? critMultiplier : 1f;
        static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");
        static CharacterBehaviour _characterBehaviour;

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
        float _multiShotTimer;
        float _moveSpeedBoostTimer;

        public bool isAtkSpdBooster;
        public float atkSpdBoosterDuration = 10;
        public float atkSpdBoosterMult = 2;

        float _atkSpdTimer;

        // Character Graphics

        BaseCharacterGraphics _graphics;
        public BaseCharacterGraphics Graphics => _graphics;

        GameObject _graphicsPrefab;
        SkinnedMeshRenderer _characterMeshRenderer;

        MaterialPropertyBlock _hitShinePropertyBlock;
        TweenCase _hitShineTweenCase;

        public CharacterStats stats;
        public CharacterStats Stats => stats;

        // Gun
        BaseGunBehavior _gunBehaviour;
        public BaseGunBehavior Weapon => _gunBehaviour;

        GameObject _gunPrefabGraphics;

        // Health
        float _currentHealth;
        float _hpBonusMultiplier = 1.3f;
        public bool isHpBonus;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => stats.Health * HpBonus;
        float HpBonus => isHpBonus ? _hpBonusMultiplier : 1f;
        public bool FullHealth => _currentHealth == stats.Health;

        public bool IsActive => _isActive;
        bool _isActive;

        public static Transform Transform => _characterBehaviour.transform;

        // Movement
        MovementSettings _movementSettings;
        MovementSettings _movementAimingSettings;

        MovementSettings _activeMovementSettings;
        public MovementSettings MovementSettings => _activeMovementSettings;

        bool _isMoving;
        float _speed = 0;

        Vector3 _movementVelocity;
        public Vector3 MovementVelocity => _movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        public bool IsCloseEnemyFound => _closestEnemyBehaviour != null;

        BaseEnemyBehavior _closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => _closestEnemyBehaviour;

        Transform _playerTarget;
        GameObject _targetRing;
        Renderer _targetRingRenderer;
        TweenCase _ringTweenCase;

        VirtualCameraCase _mainCameraCase;
        public VirtualCameraCase MainCameraCase => _mainCameraCase;

        bool _isMovementActive = false;
        public bool IsMovementActive => _isMovementActive;

        public static bool NoDamage { get; private set; } = false;

        public static bool IsDead { get; private set; }  
        public static bool IsShieldImmune { get;   set; }  

        public static SimpleCallback Died;
        public static SimpleCallback DamageToShieldImmune;
        

        #endregion

        float _stunTimer;
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
            _characterBehaviour = this;
            _hitShinePropertyBlock = new MaterialPropertyBlock();
            _isActive = false;
            enabled = false;

            var tempTarget = new GameObject("[TARGET]")
            {
                transform = {position = transform.position}
            };
            tempTarget.SetActive(true);
            _playerTarget = tempTarget.transform;

            _mainCameraCase = CameraController.GetCamera(CameraType.Main);
            enemyDetector.Initialise(this);
            _currentHealth = MaxHealth;
            healthbarBehaviour.Initialise(transform, this, true,
                CharactersController.SelectedCharacter.GetCurrentStage().HealthBarOffset);
            aimRingBehavior.Init(transform);
            _targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity);
            _targetRingRenderer = _targetRing.GetComponent<Renderer>();
            aimRingBehavior.Hide();
            IsDead = false;
            stunVfx.gameObject.SetActive(false);
        }

        public void ApplyCriticalBonus()
        {
            isCritical = true;
        }

        public void ApplyHitpointsBonus()
        {
            isHpBonus = true;
            _currentHealth = MaxHealth;
        }

        public void ApplyRespawnBonus()
        {
            respawnCount = 1;
        }

        [FormerlySerializedAs("RespawnInUse")] public bool respawnInUse;

        public void UseRespawn()
        {
            Debug.LogError("RESPAWNED");
            respawnInUse = true;
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
                _currentHealth = MaxHealth;

            isHpBonus = false;
            isCritical = false;
            IsDead = false;
            _stunTimer = 0;
            _atkSpdTimer = 0;
            moveSlowTimer = 0;
            _multiShotTimer = 0f;

            if (multishotBoostVfx) multishotBoostVfx.SetActive((false));
            if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive((false));
            if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((false));

            healthbarBehaviour.EnableBar();
            healthbarBehaviour.RedrawHealth();
            enemyDetector.Reload();
            enemyDetector.gameObject.SetActive(false);
            _graphics.DisableRagdoll();
            _graphics.Reload();
            _gunBehaviour.Reload();
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
            if (_graphics != null)
                _graphics.Unload();

            if (_playerTarget != null)
                Destroy(_playerTarget.gameObject);

            if (aimRingBehavior != null)
                Destroy(aimRingBehavior.gameObject);

            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy();
        }

        public void OnLevelLoaded()
        {
            if (_gunBehaviour != null)
                _gunBehaviour.OnLevelLoaded();
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
            _characterBehaviour.agent.enabled = false;
        }

        public float moveSlowFactor;
        public float moveBoostFactor = 1;
        bool _moveSlowed;
        const float MOVE_SLOW_DURATION = 3f;
        public float moveSlowTimer;

        public void ApplyMovementSlow(float factor)
        {
            moveSlowFactor = factor;
            _moveSlowed = true;
            moveSlowTimer = MOVE_SLOW_DURATION;
        }

        public virtual void TakeDamage(float damage)
        {
            
            if (_currentHealth <= 0) return;
            
            if (IsShieldImmune)
            {
                DamageToShieldImmune?.Invoke();
                return;
            }
            
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);
            healthbarBehaviour.OnHealthChanged();
            _mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            if (_currentHealth <= 0)
            {
                healthbarBehaviour.DisableBar();
                OnCloseEnemyChanged(null);

                _isActive = false;
                enabled = false;
                enemyDetector.gameObject.SetActive(false);
                aimRingBehavior.Hide();

                OnDeath();

                _graphics.EnableRagdoll();
                Died?.Invoke();
                Vibration.Vibrate(VibrationIntensity.Medium);
            }

            HitEffect();

            AudioController.Play(AudioController.Sounds.characterHit.GetRandomItem());

            Vibration.Vibrate(VibrationIntensity.Light);

            FloatingTextController.Spawn("PlayerHit", "-" + damage.ToString("F0"),
                transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 3.75f, Random.Range(-0.1f, 0.1f)),
                Quaternion.identity, 1f);
        }

        [Button]
        public void OnDeath()
        {
            _graphics.OnDeath();
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
            _playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        protected void HitEffect()
        {
            _hitShineTweenCase.KillActive();

            _characterMeshRenderer.GetPropertyBlock(_hitShinePropertyBlock);
            _hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white);
            _characterMeshRenderer.SetPropertyBlock(_hitShinePropertyBlock);

            _hitShineTweenCase = _characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH,
                _hitShinePropertyBlock, Color.black, 0.32f);

            _graphics.PlayHitAnimation();
        }

        #region Gun

        public void SetGun(WeaponData weaponData, bool playBounceAnimation = false, bool playAnimation = false,
            bool playParticle = false)
        {
            var gunUpgrade = UpgradesController.Get<BaseWeaponUpgrade>(weaponData.UpgradeType);
            var currentStage = gunUpgrade.GetCurrentStage();

            // Check if graphics isn't exist already
            if (_gunPrefabGraphics != gunUpgrade.WeaponPrefab)
            {
                // Store prefab link
                _gunPrefabGraphics = gunUpgrade.WeaponPrefab;

                if (_gunBehaviour)
                {
                    _gunBehaviour.OnGunUnloaded();

                    Destroy(_gunBehaviour.gameObject);
                }

                if (_gunPrefabGraphics)
                {
                    var gunObject = Instantiate(_gunPrefabGraphics);
                    gunObject.SetActive(true);

                    _gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();

                    if (_graphics)
                    {
                        _gunBehaviour.InitialiseCharacter(_graphics);
                        _gunBehaviour.PlaceGun(_graphics);

                        _graphics.SetShootingAnimation(_gunBehaviour.GetShootAnimationClip());

                        _gunBehaviour.UpdateHandRig();
                    }
                }
            }

            if (_gunBehaviour)
            {
                _gunBehaviour.Initialise(this, weaponData);

                var defaultScale = _gunBehaviour.transform.localScale;

                if (playAnimation)
                {
                    _gunBehaviour.transform.localScale = defaultScale * 0.8f;
                    _gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut);
                }

                if (playBounceAnimation)
                    _gunBehaviour.PlayBounceAnimation();

                if (playParticle)
                    _gunBehaviour.PlayUpgradeParticle();
            }

            enemyDetector.SetRadius(currentStage.RangeRadius);
            aimRingBehavior.SetRadius(currentStage.RangeRadius);
        }

        public void OnGunShooted()
        {
            _graphics.OnShoot();
        }

        #endregion

        #region Graphics

        public void SetStats(CharacterStats stats)
        {
            this.stats = stats;

            _currentHealth = stats.Health;

            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();
        }

        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            if (_graphicsPrefab == newGraphicsPrefab) return;
            _graphicsPrefab = newGraphicsPrefab;

            if (_graphics != null)
            {
                if (_gunBehaviour != null)
                    _gunBehaviour.transform.SetParent(null);

                _graphics.Unload();

                Destroy(_graphics.gameObject);
            }

            var graphicObject = Instantiate(newGraphicsPrefab, transform, true);
            graphicObject.transform.ResetLocal();
            graphicObject.SetActive(true);
            _graphics = graphicObject.GetComponent<BaseCharacterGraphics>();
            _graphics.Initialise(this);
            _movementSettings = _graphics.MovementSettings;
            _movementAimingSettings = _graphics.MovementAimingSettings;
            _activeMovementSettings = _movementSettings;
            _characterMeshRenderer = _graphics.MeshRenderer;

            if (_gunBehaviour != null)
            {
                _gunBehaviour.InitialiseCharacter(_graphics);
                _gunBehaviour.PlaceGun(_graphics);
                _graphics.SetShootingAnimation(_gunBehaviour.GetShootAnimationClip());
                _gunBehaviour.UpdateHandRig();
                Jump();
            }
            else
            {
                Tween.NextFrame(Jump, 0, false, UpdateMethod.LateUpdate);
            }

            if (playParticle)
                _graphics.PlayUpgradeParticle();

            if (playAnimation)
                _graphics.PlayBounceAnimation();
        }

        #endregion

        public void Activate(bool check = true)
        {
            if (check && _isActive) return;
            _isActive = true;

            enabled = true;
            enemyDetector.gameObject.SetActive(true);
            aimRingBehavior.Show();
            _graphics.Activate();
            NavMeshController.InvokeOrSubscribe(this);
        }

        public void Disable()
        {
            if (!_isActive) return;
            _isActive = false;

            enabled = false;
            agent.enabled = false;
            aimRingBehavior.Hide();
            _targetRing.SetActive(false);
            _targetRing.transform.SetParent(null);
            _graphics.Disable();
            _closestEnemyBehaviour = null;

            if (!_isMoving) return;
            _isMoving = false;
            _speed = 0;
        }

        public void MoveForwardAndDisable(float duration)
        {
            agent.enabled = false;
            transform.DOMove(transform.position + Vector3.forward * (_activeMovementSettings.moveSpeed * duration),
                duration).OnComplete(() => { Disable(); });
        }

        public void DisableAgent()
        {
            agent.enabled = false;
        }

        public void ActivateMovement()
        {
            _isMovementActive = true;
            aimRingBehavior.Show();
        }

        void Update()
        {
            if (_moveSlowed)
            {
                if (moveSlowTimer <= 0)
                {
                    moveSlowFactor = 1f;
                    _moveSlowed = false;
                }

                moveSlowTimer -= Time.deltaTime;
            }

            if (isMoveSpeedBooster)
            {
                if (_moveSpeedBoostTimer <= 0)
                {
                    moveBoostFactor = 1f;
                    isMoveSpeedBooster = false;
                    if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((false));
                }

                _moveSpeedBoostTimer -= Time.deltaTime;
            }

            if (isStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (!(_stunTimer <= 0)) return;
                isStunned = false;
                stunVfx.gameObject.SetActive(false);

                return;
            }

            if (_gunBehaviour)
                _gunBehaviour.UpdateHandRig();

            if (!_isActive) return;

            var joystick = Control.CurrentControl;

            if (joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.1f)
            {
                if (!_isMoving)
                {
                    _isMoving = true;
                    _speed = 0;
                    _graphics.OnMovingStarted();
                }

                var maxAlowedSpeed = Mathf.Clamp01(joystick.MovementInput.magnitude) *
                                     _activeMovementSettings.moveSpeed * moveSlowFactor * moveBoostFactor;

                if (_speed > maxAlowedSpeed)
                {
                    _speed -= _activeMovementSettings.acceleration * Time.deltaTime;
                    if (_speed < maxAlowedSpeed)
                        _speed = maxAlowedSpeed;
                }
                else
                {
                    _speed += _activeMovementSettings.acceleration * Time.deltaTime;
                    if (_speed > maxAlowedSpeed)
                        _speed = maxAlowedSpeed;
                }

                _movementVelocity = transform.forward * _speed;

                transform.position += joystick.MovementInput * (Time.deltaTime * _speed);

                _graphics.OnMoving(Mathf.InverseLerp(0, _activeMovementSettings.moveSpeed, _speed), joystick.MovementInput,
                    IsCloseEnemyFound);

                if (!IsCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(joystick.MovementInput.normalized),
                        Time.deltaTime * _activeMovementSettings.rotationSpeed);
                }
            }
            else
            {
                if (_isMoving)
                {
                    _isMoving = false;
                    _movementVelocity = Vector3.zero;
                    _graphics.OnMovingStoped();
                    _speed = 0;
                }
            }

            if (IsCloseEnemyFound)
            {
                _playerTarget.position = Vector3.Lerp(_playerTarget.position,
                    new Vector3(_closestEnemyBehaviour.transform.position.x, transform.position.y,
                        _closestEnemyBehaviour.transform.position.z),
                    Time.deltaTime * _activeMovementSettings.rotationSpeed);

                transform.LookAt(new Vector3(_playerTarget.position.x, transform.position.y, _playerTarget.position.z));
            }

            _targetRing.transform.rotation = Quaternion.identity;

            if (healthbarBehaviour)
                healthbarBehaviour.FollowUpdate();

            aimRingBehavior.UpdatePosition();
        }

        void FixedUpdate()
        {
            if (isStunned) return;
            _graphics.CustomFixedUpdate();

            if (_gunBehaviour)
                _gunBehaviour.GunUpdate();

            if (isMultishotBooster)
            {
                _multiShotTimer -= Time.fixedDeltaTime;
                if (_multiShotTimer <= 0)
                {
                    MultishotOff();
                }
            }

            if (isAtkSpdBooster)
            {
                _atkSpdTimer -= Time.fixedDeltaTime;
                if (_atkSpdTimer <= 0)
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
            if (!_isActive) return;
//if(enemyBehavior is MeleeEnemyBehaviour {isInStealth: true})return;

            if (enemyBehavior)
            {
                if (!_closestEnemyBehaviour)
                    _playerTarget.position = transform.position + transform.forward * 5;
                _activeMovementSettings = _movementAimingSettings;
                _closestEnemyBehaviour = enemyBehavior;
                _targetRing.SetActive(true);
                _targetRing.transform.rotation = Quaternion.identity;
                _ringTweenCase.KillActive();
                _targetRing.transform.SetParent(enemyBehavior.transform);
                _targetRing.transform.localScale = Vector3.one * (enemyBehavior.Stats.TargetRingSize * 1.4f);
                _targetRing.transform.localPosition = Vector3.zero;
                _ringTweenCase = _targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f)
                    .SetEasing(Ease.Type.BackIn);
                CameraController.SetEnemyTarget(enemyBehavior);
                SetTargetActive();
                return;
            }

            _activeMovementSettings = _movementSettings;
            _closestEnemyBehaviour = null;
            _targetRing.SetActive(false);
            _targetRing.transform.SetParent(null);
            CameraController.SetEnemyTarget(null);
        }

        public static BaseEnemyBehavior GetClosestEnemy() => _characterBehaviour.enemyDetector.ClosestEnemy;

        public static CharacterBehaviour GetBehaviour() => _characterBehaviour;

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        public void SetTargetActive()
        {
            _targetRingRenderer.material.color = _closestEnemyBehaviour && _closestEnemyBehaviour.Tier == EnemyTier.Elite
                ? targetRingSpecialColor
                : targetRingActiveColor;
        }

        public void SetTargetUnreachable()
        {
            _targetRingRenderer.material.color = targetRingDisabledColor;
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
                _chest = other.GetComponent<AbstractChestBehavior>();

                var dist = Vector3.Distance(transform.position, other.transform.position);
                //   Debug.LogError(other.transform.name + " dist " + dist, other.transform);
                //   Debug.LogError(other.transform.name,other.transform);
                if (dist < chestOpenDistance)
                    _chest.ChestApproached();
            }
        }

        AbstractChestBehavior _chest;
        bool _isGood;

        void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(PhysicsHelper.TAG_CHEST)) return;
            if (!_chest) return;
            if (_chest.Appriached) return;
            var dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < chestOpenDistance)
                _chest.ChestApproached();
        }

        public void ApplyStun(float duration)
        {
            Debug.LogError("Stunned");
            isStunned = true;
            _stunTimer = duration;
            stunVfx.gameObject.SetActive(true);
            StartCoroutine(PlayHit());
        }

        IEnumerator PlayHit()
        {
            while (_stunTimer > 0)
            {
                _graphics.PlayHitAnimation();
                yield return new WaitForSeconds(0.3f);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestLeft();
                _chest = null;
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
                    _currentHealth = Mathf.Clamp(_currentHealth + item.DropAmount, 0, MaxHealth);
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
            _atkSpdTimer = forever ? 99999999999999f : atkSpdBoosterDuration;
            if (atkSpeedBoostVfx) atkSpeedBoostVfx.SetActive(true);
        }

        public void MovementBonus(bool forever = false)
        {
            isMoveSpeedBooster = true;
            _moveSpeedBoostTimer = forever ? 9999999999999999f : moveSpeedBoostDuration;
            moveBoostFactor = 1.3f;
            if (moveSpeedBoostVfx) moveSpeedBoostVfx.SetActive((true));
        }

        public void MultishotBonus(bool forever = false)
        {
            isMultishotBooster = true;
            _multiShotTimer = forever ? 999999999999f : multishotBoosterDuration;
            if (multishotBoostVfx) multishotBoostVfx.SetActive((true));
        }


        [Button]
        public void Jump()
        {
            _graphics.Jump();
            _gunBehaviour.transform.localScale = Vector3.zero;
            _gunBehaviour.gameObject.SetActive(false);
        }

        public void SpawnWeapon()
        {
            _graphics.EnableRig();
            _gunBehaviour.gameObject.SetActive(true);
            _gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
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