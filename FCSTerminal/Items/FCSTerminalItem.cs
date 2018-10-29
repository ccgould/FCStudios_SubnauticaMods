using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FCSTerminal.Configuration;
using FCSTerminal.Controllers;
using FCSTerminal.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTerminal
{
    /// <summary>
    /// A class that creates the FCSTerminal
    /// </summary>
    public class FCSTerminalItem : Buildable
    {
        /// <summary>
        /// The orginal in game picture frame gameObject
        /// </summary>
        private GameObject _pictureFrameGameObject;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="classId"> Custom ID for this object</param>
        /// <param name="friendlyName">Friendly name of the object</param>
        /// <param name="description">The tooltip for the item</param>
        public FCSTerminalItem(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            
        }

        /// <summary>
        /// Run to create the FCS Terminal prefab
        /// </summary>
        public void Register()
        {
            var assetName = "FCS_Terminal";

            _storageContainer = Resources.Load<GameObject>("Submarine/Build/Locker");
            _pictureFrameGameObject = Resources.Load<GameObject>("Submarine/Build/PictureFrame");

            
            background.gameObject.SetActive(true);
            background.sprite = Helpers.ImageUtils.LoadSprite($"./ QMods /{ Information.ModName}/ Assets / screen.png");

            //ImageUtils.LoadSpriteFromFile($"./QMods/{Information.ModName}/Assets/screen.png"));

            var pictureFrame = _pictureFrameGameObject.GetComponent<PictureFrame>();
            MonoBehaviour.DestroyImmediate(pictureFrame);


            var _techType = _pictureFrameGameObject.GetComponent<TechType>();
            _techType = TechType;

        }

        public override GameObject GetGameObject()
        {

            var prefab = GameObject.Instantiate(_pictureFrameGameObject);
            prefab.name = this.ClassID;

            var canvas = TerminalPrefabShared.CreateCanvas(prefab.transform);
            

            return prefab;

        }

        protected override TechData GetBlueprintRecipe()
        {
            var tech = new TechData()
            {
                Ingredients = new List<Ingredient>
                {

                    new Ingredient(TechType.ComputerChip,2),
                    new Ingredient(TechType.AluminumOxide,1),
                    new Ingredient(TechType.Titanium,3),
                    new Ingredient(TechType.Magnetite,1),
                    new Ingredient(TechType.Gold,2),

                }
            };

            Log.Info("Created Ingredients for FCSTerminal");
            return tech;
        }


        #region Screen


        #endregion

        private GameObject _storageContainer;
        private Image background;
        public override string AssetsFolder { get; } = $@"FCSTerminal/Assets";
        public override string IconFileName { get; } = "Default.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;
        public override string HandOverText { get; } = "Click to open";
    }
}
