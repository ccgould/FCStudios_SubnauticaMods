using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;
internal class OreCrusherEffectsManager : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _smokeParticleEmitters;
    [SerializeField] private List<ParticleSystem> _bubblesParticleEmitters;

    public bool IsUnderWater()
    {
        return GetDepth() >= 7.0f;
    }

    private float GetDepth()
    {
        return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
    }


    private IEnumerator PlayFX()
    {
        EmitterState(true);
        yield return new WaitForSeconds(1f);
        EmitterState(false);
    }


    private void EmitterState(bool state)
    {
        if (IsUnderWater())
        {
            foreach (ParticleSystem emitter in _bubblesParticleEmitters)
            {
                if (state)
                {
                    emitter.Play();
                }
                else
                {
                    emitter.Stop();
                }
            }
        }
        else
        {
            foreach (ParticleSystem emitter in _smokeParticleEmitters)
            {
                if (state)
                {
                    emitter.Play();
                }
                else
                {
                    emitter.Stop();
                }
            }
        }


    }

    public void TriggerFX()
    {
        StartCoroutine(PlayFX());
    }
}
