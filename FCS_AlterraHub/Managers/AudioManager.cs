using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers
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

        public void LoadFModAssets(string id, string audioPath)
        {
            _loopingEmitter.asset = ScriptableObject.CreateInstance<FMODAsset>();
            _loopingEmitter.asset.id = id;
            _loopingEmitter.asset.path = audioPath;
        }

        #endregion

        #region public Methods    
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
