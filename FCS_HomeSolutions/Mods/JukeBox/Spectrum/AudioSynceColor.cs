using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.UI;


namespace FCS_HomeSolutions.Mods.JukeBox.Spectrum
{
    public class AudioSyncColor : AudioSyncer
    {

        private IEnumerator MoveToColor(Color _target)
        {
            Color _curr = m_img.color;
            Color _initial = _curr;
            float _timer = 0;

            while (_curr != _target)
            {
                _curr = Color.Lerp(_initial, _target, _timer / timeToBeat);
                _timer += Time.deltaTime;

                m_img.color = _curr;

                yield return null;
            }

            m_isBeat = false;
        }

        private Color RandomColor()
        {
            if (beatColors == null || beatColors.Length == 0) return Color.white;
            m_randomIndx = Random.Range(0, beatColors.Length);
            return beatColors[m_randomIndx];
        }

        public override void OnUpdate()
        {
            if (m_img == null)
            {
                FindMaterial();
                return;
            }
            base.OnUpdate();

            if (m_isBeat) return;

            var color = Color.Lerp(m_img.color, restColor, restSmoothTime * Time.deltaTime);

            m_img.SetVector("_GlowColor", color);
            m_img.SetFloat("_GlowStrength", Mathf.Clamp(AudioSpectrum.spectrumValue,0,2));
            m_img.SetFloat("_GlowStrengthNight", Mathf.Clamp(AudioSpectrum.spectrumValue, 0, 2));

            //MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
        }

        public override void OnBeat()
        {
            base.OnBeat();

            Color _c = RandomColor();

            StopCoroutine("MoveToColor");
            StartCoroutine("MoveToColor", _c);
        }

        private void FindMaterial()
        {
            m_img = MaterialHelpers.GetMaterial(GameObjectHelpers.FindGameObject(gameObject, "mesh"), AlterraHub.BaseLightsEmissiveController);
        }

        public Color[] beatColors;
        public Color restColor;

        private int m_randomIndx;
        private Material m_img;
    }
}
