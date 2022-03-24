using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Model
{
    internal class EffectsManager : MonoBehaviour
    {
        private HashSet<ParticleSystem> _smokeParticleEmitters = new();
        private HashSet<ParticleSystem> _bubblesParticleEmitters = new();
        private bool _isUnderwater;

        internal void Initialize(bool isUnderWater)
        {
            _smokeParticleEmitters.Add(gameObject.transform.Find("WhiteSmoke").GetComponent<ParticleSystem>());
            _smokeParticleEmitters.Add(gameObject.transform.Find("WhiteSmoke_1").GetComponent<ParticleSystem>());
            _bubblesParticleEmitters.Add(gameObject.transform.Find("xBubbles").GetComponent<ParticleSystem>());
            _bubblesParticleEmitters.Add(gameObject.transform.Find("xBubbles_1").GetComponent<ParticleSystem>());
            _isUnderwater = isUnderWater;
        }

        private IEnumerator PlayFX()
        {
            EmitterState(true);
            yield return new WaitForSeconds(1f);
            EmitterState(false);
        }


        private void EmitterState(bool state)
        {
            if (_isUnderwater)
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
}