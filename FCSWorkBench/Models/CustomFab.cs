using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using RootMotion;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

namespace FCSTechFabricator.Models
{
    internal class CustomFab : CustomFabricator
    {

        private static readonly Dictionary<string, string> AlterraDivisions = new Dictionary<string, string>()
        {
            {"Alterra Industrial Solutions","AIS"},
            {"Alterra Medical Solutions","AMS"},
            {"Alterra Refrigeration Solutions","ARS"},
            {"Alterra Shipping Solutions","ASS"},
            {"Alterra Storage Solutions","ASTS"},
            {"Alterra Electric","AE"}
        };

        internal static readonly Dictionary<string, ModKey> FCSMods = new Dictionary<string, ModKey>()
        {
            {"Marine Turbines",new ModKey{Key = "MT",ParentKey = "AIS"}},
            {"Deep Driller",new ModKey{Key = "DD",ParentKey = "AIS"}},
            {"Power Storage",new ModKey{Key = "PS",ParentKey = "AIS"}},
            {"SeaBreeze",new ModKey{Key = "SB",ParentKey = "ARS"}},
            {"Ex-Storage",new ModKey{Key = "ES",ParentKey = "ASTS"}},
            {"Sea Cooker",new ModKey{Key = "SC",ParentKey = "AE"}},
            {"Mini Fountain Filter",new ModKey{Key = "MFF",ParentKey = "AE"}},
            {"Mini MedBay",new ModKey{Key = "MMB",ParentKey = "AMS"}},
            {"Powercell Socket",new ModKey{Key = "PSS",ParentKey = "AIS"}},
            {"Intra-Base Teleporter",new ModKey{Key = "IBT",ParentKey = "AE"}},
            {"Quantum Teleporter",new ModKey{Key = "QT",ParentKey = "AE"}},
            {"Alterra Shipping",new ModKey{Key = "ASU",ParentKey = "ASS"}},
        };

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";

        public CustomFab(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            CreateCustomTree();
        }


        private void CreateCustomTree()
        {
            //_root = CraftTreeHandler.CreateCustomCraftTreeAndType("FCSTechFabricator", out CraftTree.Type craftType);

            //TechFabricatorCraftTreeType = craftType;

            QuickLogger.Debug($"Attempting to add {ClassID} to nodes");

            foreach (var division in AlterraDivisions)
            {
                QuickLogger.Debug($"Creating tab {division.Key}");

                AddTabNode($"{division.Value}", $"{division.Key}", new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{division.Value}Icon.png")));

                QuickLogger.Debug($"{division.Key} node tab Created");

                foreach (var fcsMod in FCSMods)
                {
                    if (fcsMod.Value.ParentKey == division.Value)
                    {
                        var icon = new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/{Mod.ModFolderName}/Assets/{fcsMod.Value.Key}Icon.png"));
                        AddTabNode(fcsMod.Value.Key, fcsMod.Key, icon);
                        QuickLogger.Debug($"Child node {fcsMod.Key} tab Created");
                    }
                }
            }
        }
    }

    internal class ModKey
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
    }
}
