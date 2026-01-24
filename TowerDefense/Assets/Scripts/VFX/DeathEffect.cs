using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private float defaultDelay = 1.0f;
    [SerializeField] private bool autoCalculateDuration = true;
    #endregion

    private void Start()
    {
        float lifeTime = defaultDelay;

        if (autoCalculateDuration)
        {
            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle != null)
            {
                float particleDuration = particle.main.duration + particle.main.startLifetime.constantMax;
                lifeTime = Mathf.Max(lifeTime, particleDuration);
            }
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                lifeTime = Mathf.Max(lifeTime, audioSource.clip.length);
            }
        }
        Destroy(gameObject, lifeTime);
    }
}
