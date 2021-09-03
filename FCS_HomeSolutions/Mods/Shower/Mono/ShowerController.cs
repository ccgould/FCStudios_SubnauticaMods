using UnityEngine;

namespace FCS_HomeSolutions.Mods.Shower.Mono
{
    internal class ShowerController : HandTarget, IHandTarget
    {
        private bool _isOn;
        private ParticleSystem _shower;
        private AudioSource _showerFx;
        private bool _wasPlaying;
        public string TurnOffText { get; set; }= "Turn Off Shower";
        public string TurnOnText { get; set; }= "Turn On Shower";
        private void Update()
        {
            if (_showerFx != null && _showerFx.isPlaying)
            {

                if (_showerFx.isPlaying && Mathf.Approximately(Time.timeScale, 0f))
                {
                    _showerFx.Pause();
                    _wasPlaying = true;
                }

                if (_wasPlaying && Time.timeScale > 0)
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
            main.SetInteractText(_isOn ? TurnOffText : TurnOnText);
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if(_isOn)
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