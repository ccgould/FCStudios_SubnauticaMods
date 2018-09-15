using System;
using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCSTerminal.Models
{
    public interface IFCSTerminalItem 
    {
        // Property signatures
        string ClassID_I { get; set; }
        TechType TechType_I { get; set; }

        #region Public Methods
        void RegisterItem(); 
        #endregion
    }

    public abstract class FCSTerminalItem : ModPrefab, IFCSTerminalItem
    {


        // This is used as the default path when we add a new resource to the game
        public const string DefaultResourcePath = "WorldEntities/Environment/Wrecks/";

        // This is used to know if we already registered our item in the game
        public bool IsRegistered = false;

        // The item resource path
        public string ResourcePath { get; set; }

         // The item root GameObject
        public GameObject GameObject { get; set; } 

        // The item TechType
        public TechType TechType_I { get; set; }

        // The items ClassID
        public string ClassID_I { get; set; }

        // The item recipe
        public TechData Recipe { get; set; }


        #region Absrtact and Virtual Methods

        public virtual void RegisterItem()
        {
            Log.Info("fgdgfgdfgdfg");
        }

        #endregion

        protected FCSTerminalItem(string classId, string prefabFileName, TechType techType = TechType.None) : base(classId, prefabFileName, techType)
        {
            
        }
    }
}
