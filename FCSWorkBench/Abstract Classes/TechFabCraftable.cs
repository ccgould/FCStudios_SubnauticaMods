using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCSTechFabricator
{
    internal abstract class TechFabCraftable : Spawnable
    {
        protected TechFabCraftable(string classId, string friendlyName, string description, bool isPlaceable = true, EquipmentType equipmentType = EquipmentType.Hand, TechType requiredForUnlock = TechType.None) : base(classId, friendlyName, description)
        {
            OnStartedPatching = Register;

            OnFinishedPatching = () =>
            {

                TechTypeID = this.TechType;
              
                if (isPlaceable)
                {
                    CraftDataHandler.SetEquipmentType(TechType, equipmentType);
                }
                
                CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());
                CraftDataHandler.AddToGroup(GroupForPDA, CategoryForPDA, TechType);
                CorePatch();
            };
        }

        private void CorePatch()
        {
            if (this.UnlockedAtStart)
            {
                KnownTechHandler.UnlockOnStart(this.TechType);
            }
            else
            {
                KnownTechHandler.SetAnalysisTechEntry(this.RequiredForUnlock, new TechType[1] { this.TechType }, $"{this.FriendlyName} blueprint discovered!");
            }
        }

        public override GameObject GetGameObject()
        {
            InstantiatedPrefab = GameObject.Instantiate<GameObject>(OriginalPrefab);

            InstantiatedPrefab.name = this.PrefabFileName;
            
            GetGameObjectExt(InstantiatedPrefab);

            PrefabIdentifier prefabID = InstantiatedPrefab.GetOrAddComponent<PrefabIdentifier>();

            prefabID.ClassId = this.ClassID;

            var techTag = InstantiatedPrefab.GetOrAddComponent<TechTag>();
            techTag.type = TechType;

            return InstantiatedPrefab;
        }

        public GameObject InstantiatedPrefab { get; set; }

        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";
        public abstract string[] StepsToFabricatorTab { get; }
        public abstract TechCategory CategoryForPDA { get; }
        public abstract TechGroup GroupForPDA { get; }
        public abstract TechType TechTypeID { get; set; }
        protected abstract TechData GetBlueprintRecipe();
        public abstract GameObject OriginalPrefab { get; set; }

        /// <summary>
        /// Override to set the <see cref="TechType"/> that must first be scanned or picked up to unlock the blueprint for this item.
        /// If not overriden, it this item will be unlocked from the start of the game.
        /// </summary>
        public virtual TechType RequiredForUnlock => TechType.None;

        /// <summary>
        /// Gets a value indicating whether <see cref="RequiredForUnlock"/> has been set to lock this blueprint behind another <see cref="TechType"/>.
        /// </summary>
        /// <value>
        ///   Returns <c>true</c> if will be unlocked from the start of the game; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="RequiredForUnlock"/>
        public bool UnlockedAtStart => this.RequiredForUnlock == TechType.None;


        public virtual void GetGameObjectExt(GameObject instantiatedPrefab) { }

        public virtual void Register() { }
    }
}
