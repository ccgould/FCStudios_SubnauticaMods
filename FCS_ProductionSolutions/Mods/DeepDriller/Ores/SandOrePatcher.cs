using System.IO;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.Ores
{
    internal class SandSpawnable : Spawnable
    {
        public SandSpawnable() : base(Mod.SandSpawnableClassID, "Sand", "Sand can be used to make glass.")
        {
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate<GameObject>(ModelPrefab.DeepDrillerSandPrefab);

            prefab.name = this.PrefabFileName;

            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
            }

            // We can pick this item
            var pickupable = prefab.AddComponent<Pickupable>();
            pickupable.isPickupable = true;
            pickupable.randomizeRotationWhenDropped = true;

            // Make the object drop slowly in water
            var wf = prefab.AddComponent<WorldForces>();
            wf.underwaterGravity = 0;
            wf.underwaterDrag = 10f;
            wf.enabled = true;

            prefab.AddComponent<TechTag>().type = TechType;

            prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;

            return prefab;
        }

        public override string AssetsFolder => Mod.GetAssetFolder();

        private static readonly SandSpawnable Singleton = new SandSpawnable();

        internal static void PatchHelper()
        {
            Singleton.Patch();
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
