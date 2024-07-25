using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public partial class ParticlesController : MonoBehaviour
    {
        [SerializeField] Particle[] particles;
        static readonly Dictionary<int, Particle> _registered = new();
        static readonly List<ParticleCase> _active = new();
        static readonly List<TweenCase> _delayed = new();
        static int _activeParticlesCount;
        public void Initialise()
        {
            foreach (var particle in particles)
                Register(particle);
            StartCoroutine(CheckForActive());
        }

        public static void Clear()
        {
            foreach (var tween in _delayed)
                tween.KillActive();
            _delayed.Clear();
            for (var i = _activeParticlesCount - 1; i >= 0; i--)
            {
                _active[i].OnDisable();
                _active.RemoveAt(i);
                _activeParticlesCount--;
            }
        }

        IEnumerator CheckForActive()
        {
            while (true)
            {
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;

                for (var i = _activeParticlesCount - 1; i >= 0; i--)
                {
                    if (_active[i] != null)
                    {
                        if (_active[i].IsForceDisabledRequired())
                            _active[i].ParticleSystem.Stop();

                        if (_active[i].ParticleSystem.IsAlive()) continue;
                        
                        _active[i].OnDisable();
                        _active.RemoveAt(i);
                        _activeParticlesCount--;
                    }
                    else
                    {
                        _active.RemoveAt(i);
                        _activeParticlesCount--;
                    }
                }
            }
        }

        static ParticleCase ActivateParticle(Particle particle, float delay = 0)
        {
            var isDelayed = delay > 0;
            var particleCase = new ParticleCase(particle, isDelayed);

            if(isDelayed)
            {
                TweenCase delayTweenCase = null;
                delayTweenCase = Tween.DelayedCall(delay, () =>
                {
                    particleCase.ParticleSystem.Play();
                    _active.Add(particleCase);
                    _activeParticlesCount++;
                    _delayed.Remove(delayTweenCase);
                });

                _delayed.Add(delayTweenCase);
                return particleCase;
            }

            _active.Add(particleCase);
            _activeParticlesCount++;
            return particleCase;
        }

        #region Register

        static int Register(Particle particle)
        {
            var particleHash = particle.ParticleName.GetHashCode();
            if (!_registered.ContainsKey(particleHash))
            {
                particle.Initialise();
                _registered.Add(particleHash, particle);
                return particleHash;
            }

            Debug.LogError(string.Format("[Particle Controller]: Particle with name {0} already register!"));

            return -1;
        }

        public static int Register(string particleName, GameObject particlePrefab)
        {
            return Register(new Particle(particleName, particlePrefab));
        }
        #endregion

        #region Play
        public static ParticleCase Play(string particleName, float delay = 0)
        {
            var particleHash = particleName.GetHashCode();
            if (_registered.ContainsKey(particleHash))
                return ActivateParticle(_registered[particleHash], delay);

            Debug.LogError($"[Particles System]: Particle with type {particleName} is missing!");
            return null;
        }

        public static ParticleCase Play(int particleHash, float delay = 0)
        {
            if (_registered.ContainsKey(particleHash))
                return ActivateParticle(_registered[particleHash], delay);

            Debug.LogError($"[Particles System]: Particle with hash {particleHash} is missing!");
            return null;
        }

        public static ParticleCase Play(Particle particle, float delay = 0)
        {
            var particleHash = particle.ParticleName.GetHashCode();
            if (_registered.ContainsKey(particleHash))
                return ActivateParticle(_registered[particleHash], delay);
            Debug.LogError($"[Particles System]: Particle with hash {particleHash} is missing!");
            return null;
        }
        #endregion

        public static int GetHash(string particleName)
            => particleName.GetHashCode();
    }
}
 