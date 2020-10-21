using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHomeSolutions.Buildables
{
    internal class SweetWaterBarPatch : DecorationEntryPatch
    {
        public SweetWaterBarPatch(string classId, string friendlyName, string description, GameObject prefab, Settings settings) : base(classId, friendlyName, description, prefab, settings)
        {

        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = _settings.AllowedOutside;
                constructable.allowedInBase = _settings.AllowedInBase;
                constructable.allowedOnGround = _settings.AllowedOnGround;
                constructable.allowedOnWall = _settings.AllowedOnWall;
                constructable.rotationEnabled = _settings.RotationEnabled;
                constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
                constructable.allowedInSub = _settings.AllowedInSub;
                constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
                constructable.model = model;
                constructable.techType = TechType;

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DecorationController>();
                prefab.AddComponent<SweetWaterBarController>();


                //Apply the glass shader here because of autosort lockers for some reason doesn't like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }
    }

    internal class SweetWaterBarController : MonoBehaviour, IConstructable
    {
        private FCSAquarium _fcsAquarium;
        private bool _runStartUpOnEnable;
        private Dictionary<string,string> _plantResourceDictionary = new Dictionary<string, string>
        {
            { "[CORAL_REEF_PLANT_MIDDLE]","WorldEntities/Doodads/Coral_reef/coral_reef_plant_middle_03"},
            { "[CORAL_REEF_G3]","WorldEntities/Doodads/Coral_reef/coral_reef_grass_03"},
            { "[CORAL_REEF_SMALL_DECO]","WorldEntities/Doodads/Coral_reef/coral_reef_small_deco_14"},
            { "[PURPLE_FAN]","WorldEntities/Doodads/Coral_reef/Coral_reef_purple_fan"},
        }; 
        
        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
                
                _runStartUpOnEnable = false;
            }
        }

        public bool CanDeconstruct(out string reason)
        {
            reason= string.Empty;
            return true;
        }

        private void Initialize()
        {
            if (_fcsAquarium == null)
            {
                var storageRoot = GameObjectHelpers.FindGameObject(gameObject, "StorageRoot");
                _fcsAquarium = gameObject.AddComponent<FCSAquarium>();
                _fcsAquarium.Initialize(storageRoot, new[] { TechType.Peeper, TechType.GarryFish, TechType.Bladderfish, TechType.HoleFish });
            }

            var plantspawns = GameObjectHelpers.FindGameObject(gameObject, "spawn_pnt");

            foreach (Transform plantGo in plantspawns.transform)
            {
                var name = plantGo.gameObject.name;

                if (_plantResourceDictionary.ContainsKey(name))
                {
                    SpawnHelper.SpawnAtPoint(_plantResourceDictionary[name], plantGo);
                }
            }


            IsInitialized = true;
        }

        public bool IsInitialized { get; set; }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
    }
}
