using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class AudioManager
    {
        #region Private Members
        private readonly FMOD_CustomLoopingEmitter _loopingEmitter;
        private bool _soundPlaying;
        private static FMODAsset _scanLoop;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public AudioManager(FMOD_CustomLoopingEmitter emitter)
        {
            if (_scanLoop == null)
            {
                _scanLoop = ScriptableObject.CreateInstance<FMODAsset>();
                _scanLoop.id = "{d321180b-6948-49c3-b109-f8b7268644eb}";
                _scanLoop.path = "event:/tools/scanner/scan_loop";
            }
            
            _loopingEmitter = emitter;

            _loopingEmitter.asset = _scanLoop;
        }
        #endregion

        #region Internal Methods    
        /// <summary>
        /// Plays the filtration machine audio.
        /// </summary>
        internal void PlayScanAudio()
        {
            if (!_soundPlaying)
            {
                _loopingEmitter.Play();
                _soundPlaying = true;
            }
        }

        /// <summary>
        /// Stops the filtration machine audio.
        /// </summary>
        internal void StopScanAudio()
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
