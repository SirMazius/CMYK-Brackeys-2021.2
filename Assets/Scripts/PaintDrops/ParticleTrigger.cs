using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    public float ActivedParts;


    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }


    public void OnParticleCollision(GameObject other)
    {
        if (other.layer == LayerMask.NameToLayer("Floor"))
        {
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

            int i = 0;

            while (i < numCollisionEvents)
            {
                if(collisionEvents[i].velocity.magnitude > AudioManager.self.paintPartSpeedSound)
                {
                    AudioManager.self.PlayAdditivelyWithOffset(SoundId.Paint_drop_hit, 0.1f);
                    return;
                }
                i++;
            }
        }
    }
}
