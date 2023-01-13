using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.JukeBox.Spectrum
{
    public class AudioSyncColor : AudioSyncer
    {
        public override void OnUpdate()
        {
            if (m_img == null)
            {
                FindMaterial();
                return;
            }
            base.OnUpdate();

            if (m_isBeat) return;

            var color = Color.Lerp(m_img.GetVector("_GlowColor"), restColor, restSmoothTime * Time.deltaTime);

            m_img.SetVector("_GlowColor", color);
            m_img.SetFloat("_GlowStrength", Mathf.Clamp(AudioSpectrum.spectrumValue,0,2));
            m_img.SetFloat("_GlowStrengthNight", Mathf.Clamp(AudioSpectrum.spectrumValue, 0, 2));
        }

        public override void OnBeat()
        {
            base.OnBeat();
            m_isBeat = false;
        }

        private void FindMaterial()
        {
            m_img = MaterialHelpers.GetMaterial(GameObjectHelpers.FindGameObject(gameObject, "mesh"), string.Empty);
        }
        
        public Color restColor;
        private Material m_img;
    }
}
