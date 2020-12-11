using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class AudioManager
    {
        #region Private Members
        private readonly FMOD_CustomLoopingEmitter _loopingEmitter;
        private bool _soundPlaying;
        private FMODAsset _scanLoop;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public AudioManager(FMOD_CustomLoopingEmitter emitter)
        {
            LoadFModAssets();

            _loopingEmitter = emitter;

            _loopingEmitter.asset = _scanLoop;
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
                    case "scan_loop":
                        QuickLogger.Debug("SCAN_LOOP found!", true);
                        this._scanLoop = fmod;
                        break;
                }
            }

            if (_scanLoop == null)
            {
                QuickLogger.Debug("Scan_Loop not found trying to search again...", true);
                Resources.Load<GameObject>("WorldEntities/Tools/Scanner");
                LoadFModAssets();
            }
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
