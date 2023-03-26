using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using SMLHelper.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract
{
    public abstract class FCSBuildableModBase : SMLHelper.Assets.Buildable, IModBase
    {
        private GameObject _prefab;
        protected FCSModItemSettings _settings;
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

        private string _modName;

        public override string AssetsFolder { get; }
        public override TechType RequiredForUnlock => TechType;
        public override bool UnlockedAtStart => false;

        public GameObject Prefab { get; private set; }

        protected FCSBuildableModBase(string modName, string prefabName,string modDir, string classId, string friendlyName) : base(classId, friendlyName, string.Empty)
        {
            _modName = modName;
            AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
            _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(),modDir);
        }

        public virtual void PatchSMLHelper() 
        {
            _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName,ClassID);
            Description = _settings.Description;
            Patch();
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var prefab = GameObject.Instantiate(_prefab);
                        
            GameObjectHelpers.AddConstructableBounds(prefab, _settings.ConstructableSize, _settings.ConstructableCenter);

            var model = prefab.FindChild("model");

            //========== Allows the building animation and material colors ==========//
            GameObjectHelpers.SetDefaultSkyApplier(prefab);
            //========== Allows the building animation and material colors ==========// 

            var lw = prefab.AddComponent<LargeWorldEntity>();
            lw.cellLevel = _settings.CellLevel;

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

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = ClassID;

            prefab.AddComponent<TechTag>().type = TechType;

            if(_settings.HasGlass)
            {
                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Main.ModSettings.ModPackID);
            }

            Prefab = prefab;

            gameObject.Set(prefab);
            yield break;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
