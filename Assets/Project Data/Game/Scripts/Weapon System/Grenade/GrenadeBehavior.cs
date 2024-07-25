using System.Collections;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class GrenadeBehavior : MonoBehaviour
    {
        public float angle = 45f;
        public float gravity = 150f;
        public bool movementSlow;
        public float movementSlowFactor = 0.2f;

        public DuoVector3 angularVelocityDuo;
        Vector3 _angularVelocity;

        [SerializeField] Rigidbody rb;
        [SerializeField] MeshRenderer sphereRenderer;
        [SerializeField] float explosionRadius;

        [SerializeField] Color startColor;
        [SerializeField] Color endColor;
        [SerializeField] Ease.Type easing;

        int _explosionHash;
        int _explosionDecalHash;
        float _duration;
        static readonly int _baseColor = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            _explosionHash = ParticlesController.GetHash("Bomber Explosion");
            _explosionDecalHash = ParticlesController.GetHash("Bomber Explosion Decal");
        }

        public void Throw(Vector3 startPosition, Vector3 targetPosition, float damage)
        {
            gameObject.SetActive(true);
            _angularVelocity = angularVelocityDuo.Random();

            transform.position = startPosition;
            rb.isKinematic = true;
            rb.useGravity = false;

            StartCoroutine(ThrowCoroutine(startPosition, targetPosition, angle, gravity, damage));

            sphereRenderer.gameObject.SetActive(true);
            sphereRenderer.material.SetColor(_baseColor, startColor);
            sphereRenderer.transform.localScale = Vector3.zero;
            sphereRenderer.material.DOColor(_baseColor, endColor, _duration + 0.5f).SetEasing(easing);
            sphereRenderer.DOScale(explosionRadius * 2, _duration + 0.25f).SetEasing(easing);
        }

        IEnumerator ThrowCoroutine(Vector3 startPosition, Vector3 targetPosition, float angle, float gravity,
            float damage)
        {
            var distance = Vector3.Distance(startPosition, targetPosition);
            var direction = (targetPosition - startPosition).normalized;

            var velocity = distance / (Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravity);

            var vx = Mathf.Sqrt(velocity) * Mathf.Cos(angle * Mathf.Deg2Rad);
            var vy = Mathf.Sqrt(velocity) * Mathf.Sin(angle * Mathf.Deg2Rad);

            _duration = distance / vx;

            var time = 0f;
            var prevPos = transform.position;

            while (time < _duration)
            {
                prevPos = transform.position;
                transform.position += Vector3.up * ((vy - gravity * time) * Time.deltaTime) +
                                      direction * (vx * Time.deltaTime);
                transform.eulerAngles += _angularVelocity * Time.deltaTime;
                time += Time.deltaTime;
                yield return null;
            }

            rb.isKinematic = false;
            rb.useGravity = true;

            var calculatedVelocity = (transform.position - prevPos) / Time.deltaTime;
            var clampedVelocity = new Vector3(Mathf.Clamp(calculatedVelocity.x, -100f, 100f),
                Mathf.Clamp(calculatedVelocity.y, -100f, 100f), Mathf.Clamp(calculatedVelocity.z, -100f, 100f));
            rb.velocity = clampedVelocity;
            rb.angularVelocity = _angularVelocity;

            yield return new WaitForSeconds(0.5f);

            var explosionCase = ParticlesController.Play(_explosionHash);
            explosionCase.SetPosition(transform.position);

            var explosionDecalCase = ParticlesController.Play(_explosionDecalHash);
            explosionDecalCase.SetPosition(transform.position).SetScale(Vector3.one * 3f)
                .SetRotation(Quaternion.Euler(-90f, 0f, 0f));
           
            AudioController.Play(AudioController.Sounds.explode);
            gameObject.SetActive(false);

            var characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (Vector3.Distance(transform.position, characterBehaviour.transform.position) <= explosionRadius)
            {
                characterBehaviour.TakeDamage(damage);
                if (movementSlow) characterBehaviour.ApplyMovementSlow(movementSlowFactor);
            }

            var aliveEnemies = ActiveRoom.GetAliveEnemies();
            foreach (var enemy in aliveEnemies)
            {
                if (!(Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)) continue;
                var directionToEnemy = (enemy.transform.position.SetY(0) - transform.position.SetY(0)).normalized;
                enemy.TakeDamage(damage, transform.position, directionToEnemy);
            }
        }
    }
}