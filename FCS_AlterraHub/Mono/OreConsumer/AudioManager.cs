using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.OreConsumer
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
                        _loopingEmitter.asset = fmod;
                        break;
                }
            }

            if (_loop == null)
            {
                QuickLogger.Debug($"{FilterAudioLoop} not found trying to search again...", true);
#if SUBNAUTICA_STABLE
                Resources.Load<GameObject>(FilterAudioLoopPath);
                if(_loop == null)
                {
                    LoadFModAssets();
                }
                
#else
                AddressablesUtility.LoadAsync<GameObject>(FilterAudioLoopPath).Completed += (prefab) =>
                {
                    var filtrationMachine =  prefab.Result.GetComponent<FiltrationMachine>();
                    if (filtrationMachine != null)
                    {
                        this._loop = filtrationMachine.workSound.asset;
                        _loopingEmitter.asset = _loop;
                    }
                };
#endif
            }
        }
        #endregion

        #region Internal Methods    
        /// <summary>
        /// Plays the machine audio.
        /// </summary>
        internal void PlayMachineAudio()
        {
            if (_loopingEmitter == null) return;
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
            if (_loopingEmitter == null) return;
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
