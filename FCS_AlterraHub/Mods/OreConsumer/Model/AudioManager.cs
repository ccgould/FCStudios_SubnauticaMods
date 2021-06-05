using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Model
{
    /// <summary>
    /// A class that handles the audio of the oxstation.
    /// </summary>
    internal class AudioManager
    {
        #region Private Members
        private readonly FMOD_CustomLoopingEmitter _loopingEmitter;
        private bool _soundPlaying;
        private FMODAsset _loop;
        private const string FilterAudioLoop = "water_filter_loop";
        private const string FilterAudioLoopPath = "Submarine/Build/FiltrationMachine";
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        internal AudioManager(FMOD_CustomLoopingEmitter emitter)
        {
            LoadFModAssets();

            _loopingEmitter = emitter;

            _loopingEmitter.asset = _loop;
        }
        #endregion

        #region Private Methods
        private void LoadFModAssets()
        {
            FMODAsset[] fmods = Resources.FindObjectsOfTypeAll<FMODAsset>();

            foreach (FMODAsset fmod in fmods)
            {
                switch (fmod.name.ToLower())
                {
                    case FilterAudioLoop:
                        QuickLogger.Debug($"{FilterAudioLoop} found!", true);
                        this._loop = fmod;
                        break;
                }
            }

            if (_loop == null)
            {
                QuickLogger.Debug($"{FilterAudioLoop} not found trying to search again...", true);
                Resources.Load<GameObject>(FilterAudioLoopPath);
                LoadFModAssets();
            }
        }
        #endregion

        #region Internal Methods    
        /// <summary>
        /// Plays the machine audio.
        /// </summary>
        internal void PlayMachineAudio()
        {
            if (!_soundPlaying && QPatch.Configuration.PlaySFX)
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
            if (_soundPlaying)
            {
                _loopingEmitter.Stop();
                _soundPlaying = false;
                QuickLogger.Debug("Stopping Audio", true);
            }
        }
        #endregion
    }
}
