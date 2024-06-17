using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PAA_particleTrigger : MonoBehaviour
{
    public ParticleSystem powerup_particles;
    
    // Start is called before the first frame update
   
    void showParticles()
    {
        powerup_particles.Play();
    }

    void hideParticles()
    {
        powerup_particles.Stop();
    }

}
