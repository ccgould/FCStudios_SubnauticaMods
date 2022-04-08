using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.MatterAnalyzer.Mono
{
    internal class AudioManager
    {
        #region Private Members
        private readonly FMOD_CustomLoopingEmitter _loopingEmitter;
        private bool _soundPlaying;
        private static FMODAsset _loop;
        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor 
        /// </summary>
        internal AudioManager(FMOD_CustomLoopingEmitter emitter)
        {
            if (_loop == null)
            {
                _loop = ScriptableObject.CreateInstance<FMODAsset>();
                _loop.id = "{9acfd1ce-b2cd-4f93-9ba2-1f5e95dd0355}";
                _loop.path = "event:/sub/base/water_filter_loop";
            }

            _loopingEmitter = emitter;

            _loopingEmitter.asset = _loop;
        }

        #endregion

        #region Internal Methods    
        /// <summary>
        /// Plays the machine audio.
        /// </summary>
        internal void PlayMachineAudio()
        {
            if (!_soundPlaying && QPatch.Configuration.MatterAnalyzerPlaySFX)
            {
                _loopingEmitter.Play();
                _soundPlaying = true;
                QuickLogger.Debug("Playing Audio", true);
            }
        }

        /// <summary>
        /// Stops the machine audio.
        /// </summary>
        internal void StopMachineAudio()
        {
            if (_soundPlaying && QPatch.Configuration.MatterAnalyzerPlaySFX)
            {
                _loopingEmitter.Stop();
                _soundPlaying = false;
                QuickLogger.Debug("Stopping Audio", true);
            }
        }
        #endregion
    }
}
