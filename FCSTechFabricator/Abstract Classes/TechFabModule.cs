using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Helpers;
using FCSTechFabricator.Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCSTechFabricator.Abstract_Classes
{
    public abstract class TechFabModule : Spawnable
    {
        internal static TechType TechTypeID { get; private set; }
        
        public override string AssetsFolder { get; } = "FCSTechFabricator/Assets";

        public TechFabModule(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            OnFinishedPatching = () =>
            {
                TechTypeID = this.TechType;
                //Add the new TechType Hand Equipment type
                CraftDataHandler.SetEquipmentType(TechType, EquipmentType.CyclopsModule);
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
            GameObject prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule));

            prefab.name = this.PrefabFileName;

            prefab.AddComponent<FCSTechFabricatorTag>();

            return prefab;
        }

        public virtual TechData GetBlueprintRecipe()
        {
            return IngredientHelper.GetCustomRecipe(ClassID);
        }

        public abstract TechCategory CategoryForPDA { get; }

        public abstract TechGroup GroupForPDA { get; }

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
    }
}
