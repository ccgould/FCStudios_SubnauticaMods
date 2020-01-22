using FCSCommon.Utilities;
using UnityEngine;

namespace FCSCommon.Controllers
{
    public class AudioManager
    {
        #region Private Members
        private readonly FMOD_CustomLoopingEmitter _loopingEmitter;
        private bool _soundPlaying;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public AudioManager(FMOD_CustomLoopingEmitter emitter)
        {
            _loopingEmitter = emitter;
        }
        #endregion

        #region Private Methods

        public void LoadFModAssets(string audioPath, string trackName)
        {
            int Count = 0;
            while (Count < 2)
            {
                FMODAsset[] fMods = Resources.FindObjectsOfTypeAll<FMODAsset>();

                FMODAsset scanLoop = null;
                foreach (FMODAsset fmod in fMods)
                {
                    if (fmod.name.ToLower() != trackName) continue;
                    QuickLogger.Debug($"{trackName} found!", true);
                    scanLoop = fmod;
                }

                if (scanLoop != null)
                {
                    _loopingEmitter.asset = scanLoop;
                }
                else
                {
                    QuickLogger.Debug($"{trackName} not found trying to search again...", true);
                    Resources.Load<GameObject>(audioPath);
                    Count++;
                    continue;
                }

                break;
            }
            if(Count == 2)
            {
                QuickLogger.Debug($"{trackName} not found", true);
            }
        }

        #endregion

        #region Internal Methods    
        /// <summary>
        /// Plays the audio.
        /// </summary>
        public void PlayAudio()
        {
            if (!_soundPlaying)
            {
                _loopingEmitter.Play();
                _soundPlaying = true;
            }
        }

        /// <summary>
        /// Stops the audio.
        /// </summary>
        public void StopAudio()
        {
            if (_soundPlaying)
            {
                _loopingEmitter.Stop();
                _soundPlaying = false;
            }
        }
        #endregion
    }
}
