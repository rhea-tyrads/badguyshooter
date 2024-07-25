using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using UI.Particle;

    public class ExperienceStarsManager : MonoBehaviour
    {
        readonly string _trailPoolName = "Custom UI Trail";
        readonly string _particlePoolName = "Custom UI Particle";

        [Header("Data")]
        [SerializeField] ExperienceStarsFlightData starsData;

        [Header("Stars")]
        [SerializeField] RectTransform starsHolder;

        [Space]
        [SerializeField] GameObject starUIPrefab;

        [Space]
        [SerializeField] Transform starIconTransform;
        [SerializeField] JuicyBounce starIconBounce;

        [Header("Particles")]
        [SerializeField] RectTransform particlesParent;

        [Space]
        [SerializeField] GameObject trailPrefab;
        [SerializeField] GameObject particlePrefab;


        PoolGeneric<UIParticleTrail> _trailPool;
        PoolGeneric<UIParticle> _particlePool;

        Pool _starsPool;


        List<ExpStarData> _starsInfo = new();
        System.Action _complete;

        ExperienceUIController _experienceUIController;

        public void Awake()
        {
            AssignPools();
        }

        public void Initialise(ExperienceUIController experienceUIController)
        {
            this._experienceUIController = experienceUIController;

            starIconBounce.Initialise(starIconTransform);
        }

        void AssignPools()
        {
            if (PoolManager.PoolExists(_trailPoolName))
                _trailPool = PoolManager.GetPoolByName<UIParticleTrail>(_trailPoolName);
            else
                _trailPool = new PoolGeneric<UIParticleTrail>(new PoolSettings
                {
                    name = _trailPoolName,
                    singlePoolPrefab = trailPrefab,
                    size = 5,
                    objectsContainer = particlesParent,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });

            if (PoolManager.PoolExists(_trailPoolName))
                _particlePool = PoolManager.GetPoolByName<UIParticle>(_particlePoolName);
            else
                _particlePool = new PoolGeneric<UIParticle>(new PoolSettings
                {
                    name = _particlePoolName,
                    singlePoolPrefab = particlePrefab,
                    size = 100,
                    objectsContainer = particlesParent,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });

            if (PoolManager.PoolExists(starUIPrefab.name))
                _starsPool = PoolManager.GetPoolByName(starUIPrefab.name);
            else
                _starsPool = PoolManager.AddPool(new PoolSettings(starUIPrefab.name, starUIPrefab, 3, true, transform));
        }

        public void PlayXpGainedAnimation(int starsAmount, Vector3 worldPos, System.Action complete = null)
        {
            this._complete = complete;

            starsAmount = Mathf.Clamp(starsAmount, 1, 10);

            for (int i = 0; i < starsAmount; i++)
            {
                RectTransform starRect = _starsPool.Get().GetComponent<RectTransform>();

                starRect.SetParent(transform.parent);
                starRect.anchoredPosition = Camera.main.WorldToScreenPoint(worldPos) + new Vector3(Random.Range(-25f, 25f), Random.Range(-25f, 25f), 0f);
                starRect.SetParent(starsHolder);

                Vector2 startDirection = Random.onUnitSphere;
                Vector2 endPoint = Vector2.zero;

                var data = new ExpStarData()
                {
                    Star = starRect,

                    StartPoint = starRect.anchoredPosition,
                    MiddlePoint = starRect.anchoredPosition + startDirection * starsData.FirstStageDistance,

                    Key1 = starRect.anchoredPosition + startDirection * starsData.Key1,
                    Key2 = endPoint + starsData.Key2,

                    EndPoint = endPoint,

                    StartTime = Time.time,
                    Duration1 = starsData.FirstStageDuration,
                    Duration2 = starsData.SecondStageDuration
                };

                data.SetCurves(starsData);

                var trail = _trailPool.GetPooledComponent();

                trail.SetPool(_particlePool);

                trail.AnchoredPos = starRect.anchoredPosition;
                trail.transform.localScale = Vector3.one;

                data.SetTrail(trail);

                _starsInfo.Add(data);
            }
        }

        void Update()
        {
            if (_starsInfo.IsNullOrEmpty())
                return;

            for (int i = 0; i < _starsInfo.Count; i++)
            {
                var data = _starsInfo[i];

                if (data.Update())
                {
                    _starsInfo.RemoveAt(i);
                    i--;

                    starIconBounce.Bounce();

                    _experienceUIController.OnStarHitted();
                }
            }

            if (_starsInfo.IsNullOrEmpty())
                _complete?.Invoke();
        }

        [Button]
        public void Spawn2Stars()
        {
            ExperienceController.Add(2);
        }
        [Button]
        public void Spawn5Stars()
        {
            ExperienceController.Add(5);
        }
        [Button]
        public void Spawn10Stars()
        {
            ExperienceController.Add(10);
        }

        class ExpStarData
        {
            public RectTransform Star;

            public Vector2 StartPoint;
            public Vector2 MiddlePoint;

            public Vector2 Key1;
            public Vector2 Key2;

            public Vector2 EndPoint;

            public float StartTime;
            public float Duration1;
            public float Duration2;

            ExperienceStarsFlightData _data;

            UIParticleTrail _trail;

            public void SetCurves(ExperienceStarsFlightData data)
            {
                this._data = data;
            }

            public void SetTrail(UIParticleTrail trail)
            {
                this._trail = trail;

                trail.Init();
                trail.IsPlaying = true;
            }

            public bool Update()
            {
                var time = Time.time - StartTime;

                if (time > Duration1)
                {
                    var t = (time - Duration1) / Duration2;

                    if (t >= 1)
                    {
                        Star.gameObject.SetActive(false);

                        _trail.DisableWhenReady = true;
                        _trail.IsPlaying = false;

                        return true;
                    }

                    SecondStageUpdate(t);
                }
                else
                {
                    var t = time / Duration1;

                    FirstStageUpdate(t);
                }

                return false;
            }

            public void FirstStageUpdate(float t)
            {
                var prevPos = Star.anchoredPosition;

                Star.anchoredPosition = Vector2.Lerp(StartPoint, MiddlePoint, _data.PathCurve1.Evaluate(t));
                Star.localScale = Vector3.one * _data.StarsScale1.Evaluate(t);

                _trail.NormalizedVelocity = (Star.anchoredPosition - prevPos).normalized;

                _trail.AnchoredPos = Star.anchoredPosition;
            }

            public void SecondStageUpdate(float t)
            {
                var prevPos = Star.anchoredPosition;

                Star.anchoredPosition = Bezier.EvaluateCubic(MiddlePoint, Key1, Key2, EndPoint, _data.PathCurve2.Evaluate(t));
                Star.localScale = Vector3.one * _data.StarsScale2.Evaluate(t);

                _trail.NormalizedVelocity = (Star.anchoredPosition - prevPos).normalized;

                _trail.AnchoredPos = Star.anchoredPosition;
            }
        }
    }
}