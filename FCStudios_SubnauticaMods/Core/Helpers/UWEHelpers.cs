using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_AlterraHub.Core.Helpers
{
    public static class UWEHelpers
    {
        /// <summary>
        /// Creates a default UWE <see cref="StorageContainer". Warningdisabled and enables object./>
        /// </summary>
        /// <param name="prefab">The object to add a storage container component.</param>
        /// <param name="storageRoot">The storage root.This is were items will be held.</param>
        /// <param name="classID">The class id of the prefab this helps the <see cref="ChildObjectIdentifier"/> to save and load the items.</param>
        /// <param name="storageLabel">The label that displays with the storage is open.</param>
        /// <param name="width">The width of the storage container</param>
        /// <param name="height">The height of the storage container</param>
        public static StorageContainer CreateStorageContainer(GameObject prefab, GameObject storageRoot, string classID, string storageLabel, int width, int height, bool forceAdd = false)
        {
            if (storageRoot == null)
            {
                storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.parent = prefab.transform;
                UWE.Utils.ZeroTransform(storageRoot);
            }

            //We are disabling the gameObject so the settings can be set before Awake() is called.
            prefab.SetActive(false);

            //Create the storage container
            var storage = forceAdd ? prefab.AddComponent<StorageContainer>() : prefab.EnsureComponent<StorageContainer>();
            storage.prefabRoot = prefab;
            storage.width = width;
            storage.height = height;
            storage.storageLabel = storageLabel;
            storage.preventDeconstructionIfNotEmpty = true;

            //Make sure a ChildObjectIdentifier component is attached for saving and loading children.
            var childObjectIdentifier = storageRoot.EnsureComponent<ChildObjectIdentifier>();
            childObjectIdentifier.classId = classID;
            storage.storageRoot = childObjectIdentifier;

            //everything is set so re-enable the object
            prefab.SetActive(true);

            return storage;
        }

        public static bool RequiresSurvival()
        {
#if SUBNAUTICA
            return GameModeUtils.RequiresSurvival();
#else 
            return GameModeManager.GetOption<bool>(GameOption.Story);
#endif
        }


        /// <summary>
        /// Removes ingredients from the <see cref="ItemsContainer"/>  and destroys it.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="techType"></param>
        public static bool ConsumeIngredientsFor(ItemsContainer container, TechType techType)
        {
            var techData = CraftData.techData[techType];
            QuickLogger.Debug($"TechData: {techData?.ingredientCount} | {Language.main.Get(techType)}", true);

            var result = CheckIfAllIngredientsAreAvailable(container, techData);

            if (!result)
                goto end_operation;

            if (techData != null)
            {
                foreach (CraftData.Ingredient ingredient in techData._ingredients)
                {
                    for (int i = 0; i < ingredient.amount; i++)
                    {
                        var item = container.DestroyItem(ingredient.techType);
                        if (item) continue;
                        QuickLogger.Error($"Failed to pull item {Language.main.Get(techType)} from storage container");
                        goto end_operation;
                    }
                }

                return true;
            }

        end_operation:

            return false;

        }

        public static bool CheckIfAllIngredientsAreAvailable(ItemsContainer container, CraftData.TechData techData)
        {
            foreach (CraftData.Ingredient ingredient in techData._ingredients)
            {
                if (container.GetCount(ingredient.techType) < ingredient.amount)
                {
                    return false;
                }
            }
            return true;
        }


        public static bool RequiresPower()
        {
#if SUBNAUTICA
            return GameModeUtils.RequiresPower();
#else 
            return GameModeManager.GetOption<bool>(GameOption.TechnologyRequiresPower);
#endif
        }

        public static bool RequiresIngredients()
        {
#if SUBNAUTICA
            return GameModeUtils.RequiresIngredients();
#else 
            return GameModeManager.GetOption<bool>(GameOption.CraftingRequiresResources);
#endif
        }
    }
}
