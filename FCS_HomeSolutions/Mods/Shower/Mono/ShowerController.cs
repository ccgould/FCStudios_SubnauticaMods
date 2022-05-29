using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Shower.Mono
{
    internal class ShowerController : HandTarget, IHandTarget
    {
        private bool _isOn;
        private ParticleSystem _shower;
        private AudioSource _showerFx;
        private bool _wasPlaying;
        public string TurnOffText { get; set; } = "Turn Off Shower";
        public string TurnOnText { get; set; } = "Turn On Shower";

        private void Update()
        {
            if (_showerFx != null)
            {
                if (_showerFx.isPlaying && WorldHelpers.CheckIfPaused())
                {
                    _showerFx.Pause();
                    _wasPlaying = true;
                }

                if (_wasPlaying && !WorldHelpers.CheckIfPaused())
                {
                    _showerFx.Play();
                    _wasPlaying = false;
                }
            }
        }

        private void Start()
        {
            _shower = gameObject.transform.parent.GetComponentInChildren<ParticleSystem>();
            _showerFx = gameObject.transform.parent.GetComponentInChildren<AudioSource>();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

#if SUBNAUTICA
                main.SetInteractText(
#else
            main.SetTextRaw(HandReticle.TextType.Use,
#endif
                _isOn ? TurnOffText : TurnOnText);
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_isOn)
            {
                _shower.Stop();
                _showerFx?.Stop();
            }
            else
            {
                _shower.Play();
                _showerFx?.Play();
            }

            _isOn ^= true;
        }
    }
}