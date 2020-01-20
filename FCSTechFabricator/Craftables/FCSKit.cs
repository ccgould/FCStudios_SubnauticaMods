using System;
using System.IO;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Craftables
{
    public class FCSKit : FcCraftable
    {

        private static GameObject _gameObjectKit;
        private static bool _kitRegistered;
#if SUBNAUTICA
        private TechData _ingredients;
#elif BELOWZERO
        private RecipeData _ingredients;
#endif
        private string _assetFolder = Mod.GetAssetPath();
        private string _iconFileName = "Kit_FCS";


#if SUBNAUTICA
        public FCSKit(string classId, string friendlyName, FcCraftingTab parentTab, TechData ingredients) :
            base(classId, friendlyName, $"A kit that allows you to build one {friendlyName} Unit", parentTab)
        {
            _ingredients = ingredients;
            OnFinishedPatching += () =>
            {
                GetKitPrefab();
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }
#elif BELOWZERO
        public FCSKit(string classId, string friendlyName, FcCraftingTab parentTab, RecipeData ingredients) :
            base(classId, friendlyName, $"A kit that allows you to build one {friendlyName} Unit", parentTab)
        {
            _ingredients = ingredients;
            OnFinishedPatching += () =>
            {
                GetKitPrefab();
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.Hand);
            };
        }

#endif

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_gameObjectKit);

                if (SetKitLabel(prefab))
                {
                    prefab.name = this.PrefabFileName;

                    PrefabIdentifier prefabID = prefab.EnsureComponent<PrefabIdentifier>();

                    prefabID.ClassId = this.ClassID;

                    var techTag = prefab.EnsureComponent<TechTag>();
                    techTag.type = TechType;

                    var center = new Vector3(0f, 0.2518765f, 0f);
                    var size = new Vector3(0.5021304f, 0.5062426f, 0.5044461f);

                    GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                    //Add the FCSTechFabricatorTag component
                    prefab.AddComponent<FCSTechFabricatorTag>();

                    return prefab;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return _ingredients;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png")));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return _ingredients;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{_iconFileName}.png"));
        }
#endif

        private bool SetKitLabel(GameObject prefab)
        {
            var canvasObject = prefab.GetComponentInChildren<Canvas>().gameObject;
            if (canvasObject == null)
            {
                QuickLogger.Error("Could not find the canvas");
                return false;
            }

            var label = canvasObject.FindChild("Screen").FindChild("Label").GetComponent<Text>();
            label.text = FriendlyName;
            return true;
        }

        internal bool GetKitPrefab()
        {
            AssetBundle assetBundle = this.AssetBundle;

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }
            
            //We have found the asset bundle and now we are going to continue by looking for the model.
            _gameObjectKit = assetBundle.LoadAsset<GameObject>("UnitContainerKit");

            //If the prefab isn't null lets add the shader to the materials
            if (_gameObjectKit != null)
            {
                QuickLogger.Debug($"UnitContainerKit Prefab Found!");
                
                ApplyKitComponents();

                //Lets apply the material shader
                ApplyKitShaders(_gameObjectKit,assetBundle);
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }

            return true;
        }

        private void ApplyKitComponents()
        {
            if (!_kitRegistered)
            {
                //Find the screen on the model
                var model = _gameObjectKit.GetComponentInChildren<Canvas>().gameObject;
                model.FindChild("Screen").SetActive(true);

                //Make the rigid body isKinematic so it doesnt shake the cyclops
                var rb = _gameObjectKit.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                // Set collider
                var collider = _gameObjectKit.GetComponent<Collider>();
                collider.enabled = true;
                collider.isTrigger = true;

                // Make the object drop slowly in water
                var wf = _gameObjectKit.EnsureComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;

                // Add fabricating animation
                var fabricatingA = _gameObjectKit.EnsureComponent<VFXFabricating>();
                fabricatingA.localMinY = -0.1f;
                fabricatingA.localMaxY = 0.6f;
                fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
                fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
                fabricatingA.scaleFactor = 1.0f;

                //// Set proper shaders (for crafting animation)
                //Shader marmosetUber = Shader.Find("MarmosetUBER");
                var renderer = _gameObjectKit.GetComponentInChildren<Renderer>();
                //renderer.material.shader = marmosetUber;

                // Update sky applier
                var applier = _gameObjectKit.EnsureComponent<SkyApplier>();
                applier.renderers = new Renderer[] { renderer };
                applier.anchorSky = Skies.Auto;

                // We can pick this item
                var pickupable = _gameObjectKit.EnsureComponent<Pickupable>();
                pickupable.isPickupable = true;
                pickupable.randomizeRotationWhenDropped = true;

                //Allow this kit to be placed on surfaces in these situations
                var placeTool = _gameObjectKit.EnsureComponent<PlaceTool>();
                placeTool.allowedInBase = true;
                placeTool.allowedOnBase = false;
                placeTool.allowedOnCeiling = false;
                placeTool.allowedOnConstructable = true;
                placeTool.allowedOnGround = true;
                placeTool.allowedOnRigidBody = true;
                placeTool.allowedOnWalls = false;
                placeTool.allowedOutside = false;
                placeTool.rotationEnabled = true;
                placeTool.enabled = true;
                placeTool.hasAnimations = false;
                placeTool.hasBashAnimation = false;
                placeTool.hasFirstUseAnimation = false;
                placeTool.mainCollider = collider;
                placeTool.pickupable = pickupable;
                placeTool.drawTime = 0.5f;
                placeTool.dropTime = 1;
                placeTool.holsterTime = 0.35f;

                _kitRegistered = true;
            }
        }

        public void ChangeIconLocation(string assetFolder,string iconFileName)
        {
            _assetFolder = assetFolder;
            _iconFileName = iconFileName;
        }

        internal static void ApplyKitShaders(GameObject prefab, AssetBundle assetBundle)
        {
            MaterialHelpers.ApplyEmissionShader("UnitContainerKit_BaseColor", "UnitContainerKit_Emissive", prefab, assetBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyAlphaShader("UnitContainerKit_BaseColor", prefab);
            MaterialHelpers.ApplyNormalShader("UnitContainerKit_BaseColor", "UnitContainerKit_Norm", prefab, assetBundle);
            MaterialHelpers.ApplySpecShader("UnitContainerKit_BaseColor", "UnitContainerKit_Spec", prefab, 1f, 6f, assetBundle);
        }

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        protected override string AssetBundleName => Mod.AssetBundleName;
        public override string IconFileName => _iconFileName;
        public override string AssetsFolder => _assetFolder;
    }
}
