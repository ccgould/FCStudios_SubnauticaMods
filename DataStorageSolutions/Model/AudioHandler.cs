using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Model
{
    internal class DSSAudioHandler
    {
        #region Private Members
        private FMODAsset doorOpen;
        private FMODAsset doorClose;
        private bool allowedToPlaySounds;
        private Transform _transform;

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public DSSAudioHandler(Transform transform)
        {
            _transform = transform;
            LoadFModAssets();
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
                    case "cyclops_door_close":
                        QuickLogger.Debug("cyclops_door_close found!", true);
                        this.doorClose = fmod;
                        break;

                    case "cyclops_door_open":
                        QuickLogger.Debug("cyclops_door_open found!", true);
                        this.doorOpen = fmod;
                        break;
                }
            }

            if (doorClose == null)
            {
                QuickLogger.Debug("cyclops_door_close not found trying to search again...", true);
                Resources.Load<GameObject>("/sub/cyclops/cyclops_door_close");
                LoadFModAssets();
            }

            if (doorOpen == null)
            {
                QuickLogger.Debug("cyclops_door_open not found trying to search again...", true);
                Resources.Load<GameObject>("/sub/cyclops/cyclops_door_open");
                LoadFModAssets();
            }

            allowedToPlaySounds = true;
        }
        #endregion

        #region Internal Methods    
        internal void PlaySound(bool doorState)
        {
            if (!this.allowedToPlaySounds)
                return;
            FMODAsset asset = !doorState ? this.doorOpen : this.doorClose;
            if (!(asset != null))
                return;
            FMODUWE.PlayOneShot(asset, _transform.position);
        }
        #endregion
    }
}

