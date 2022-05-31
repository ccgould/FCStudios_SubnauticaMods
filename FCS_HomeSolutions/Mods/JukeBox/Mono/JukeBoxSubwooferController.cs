using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class JukeBoxSubwooferController : JukeBoxSpeakerController,IHandTarget
    {
        public void OnHandClick(GUIHand hand)
        {
            _audio.mute = !_audio.mute;
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;

            base.OnHandHover(hand);

            var data = new[]
            {
                $"Volume Level: {_audio.volume * 100f}",
                $"Is Muted: {_audio.mute}"
            };

            data.HandHoverPDAHelperEx(GetTechType());
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    ChangeVolume(0.1f);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    ChangeVolume(0.1f, false);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    ChangeVolume(0.01f);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    ChangeVolume(0.01f,false);
                }
            }
        }

        private void ChangeVolume(float amount,bool increase = true)
        {
            float volume;
            if (increase)
            {
                volume = _audio.volume + amount;
            }
            else
            {
                volume = _audio.volume - amount;
            }
            _audio.volume = Mathf.Clamp(volume,0f,1f);
        }
    }
}
