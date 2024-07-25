using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public partial class ParticlesController : MonoBehaviour
    {
        [SerializeField] Particle[] particles;
        static readonly Dictionary<int, Particle> Registered = new();
        static readonly List<ParticleCase> Active = new();
        static readonly List<TweenCase> Delayed = new();
        static int _activeParticlesCount;
      

        public void Initialise()
        {
            foreach (var particle in particles)
                Register(particle);

            StartCoroutine(CheckForActive());
        }

        public static void Clear()
        {
            foreach (var tween in Delayed)
                tween.KillActive();

            Delayed.Clear();

            for (var i = _activeParticlesCount - 1; i >= 0; i--)
            {
                Active[i].OnDisable();
                Active.RemoveAt(i);
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
                    if (Active[i] != null)
                    {
                        if (Active[i].IsForceDisabledRequired())
                            Active[i].ParticleSystem.Stop();

                        if (Active[i].ParticleSystem.IsAlive()) continue;
                        
                        Active[i].OnDisable();
                        Active.RemoveAt(i);
                        _activeParticlesCount--;
                    }
                    else
                    {
                        Active.RemoveAt(i);
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
                    Active.Add(particleCase);
                    _activeParticlesCount++;
                    Delayed.Remove(delayTweenCase);
                });

                Delayed.Add(delayTweenCase);
                return particleCase;
            }

            Active.Add(particleCase);
            _activeParticlesCount++;
            return particleCase;
        }

        #region Register

        static int Register(Particle particle)
        {
            var particleHash = particle.ParticleName.GetHashCode();
            if (!Registered.ContainsKey(particleHash))
            {
                particle.Initialise();
                Registered.Add(particleHash, particle);
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
            if (Registered.ContainsKey(particleHash))
                return ActivateParticle(Registered[particleHash], delay);

            Debug.LogError($"[Particles System]: Particle with type {particleName} is missing!");
            return null;
        }

        public static ParticleCase Play(int particleHash, float delay = 0)
        {
            if (Registered.ContainsKey(particleHash))
                return ActivateParticle(Registered[particleHash], delay);

            Debug.LogError($"[Particles System]: Particle with hash {particleHash} is missing!");
            return null;
        }

        public static ParticleCase Play(Particle particle, float delay = 0)
        {
            var particleHash = particle.ParticleName.GetHashCode();
            if (Registered.ContainsKey(particleHash))
                return ActivateParticle(Registered[particleHash], delay);
            Debug.LogError($"[Particles System]: Particle with hash {particleHash} is missing!");
            return null;
        }
        #endregion

        public static int GetHash(string particleName)
            => particleName.GetHashCode();
    }
}
 