using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    public class VoiceNotificationSystem : MonoBehaviour
    {

        public static VoiceNotificationSystem main;
        private static Dictionary<string,VoiceData> _voiceNotifications = new Dictionary<string, VoiceData>();
        public float minInterval;
        private float timeNextPlay = -1f;

        private void Awake()
        {
            main = this;
        }

        public static void RegisterVoice(string audioPath, string audioText)
        {
            var sound = AudioUtils.CreateSound(audioPath);
            var newKey = Path.GetFileNameWithoutExtension(audioPath) + "_key";
            if (!_voiceNotifications.ContainsKey(newKey))
            {
                _voiceNotifications.Add(newKey, new VoiceData
                {
                    Text = audioText, 
                    Sound = sound
                });
            }
        }

        public bool GetCanPlay()
        {
            return DayNightCycle.main.timePassedAsFloat >= this.timeNextPlay;
        }

        public bool Play(string notificationKey, params object[] args)
        {
            if (_voiceNotifications.ContainsKey(notificationKey))
            {
                var sound = _voiceNotifications[notificationKey].Sound;
                var text = _voiceNotifications[notificationKey].Text;

                float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
                if (timePassedAsFloat < this.timeNextPlay)
                {
                    return false;
                }
                this.timeNextPlay = timePassedAsFloat + this.minInterval;
                if (!string.IsNullOrEmpty(text))
                {
                    Subtitles.main.Add(text, args);
                }
                AudioUtils.PlaySound(sound, SoundChannel.Voice);
                return true;
            }

            return false;
        }

        public bool Play(string notificationKey, string customString, params object[] args)
        {
            if (_voiceNotifications.ContainsKey(notificationKey))
            {
                var sound = _voiceNotifications[notificationKey].Sound;

                float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
                if (timePassedAsFloat < this.timeNextPlay)
                {
                    return false;
                }
                this.timeNextPlay = timePassedAsFloat + this.minInterval;
                if (!string.IsNullOrEmpty(customString))
                {
                    Subtitles.main.Add(customString, args);
                }
                AudioUtils.PlaySound(sound, SoundChannel.Voice);
                return true;
            }

            return false;
        }
    }

    internal struct VoiceData
    {
        public string Text { get; set; }
        public Sound Sound { get; set; }
    }
}
