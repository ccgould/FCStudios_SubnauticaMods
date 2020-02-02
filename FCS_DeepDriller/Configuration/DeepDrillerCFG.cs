using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations
        public StorageConfig StorageSize { get; set; } = new StorageConfig { Height = 8, Width = 6 };

        public float PowerDraw { get; set; } = 0.1f;
        public float SolarCapacity { get; set; } = 125;
        public int DrillOrePerDay { get; set; } = 12;
        public int Mk1OrePerDay { get; set; } = 15;
        public int Mk2OrePerDay { get; set; } = 22;
        public int Mk3OrePerDay { get; set; } = 30;

        public Dictionary<string, List<string>> BiomeOres { get; set; } = new Dictionary<string, List<string>>
        {
            {"safeshallows", new List<string>
            {
                "Copper",
                "Lead",
                "Quartz",
                "Titanium",
                "Silver",
                "Gold"
            }},
            {"kelpforest", new List<string>
            {
                "Copper",
                "Lead",
                "Quartz",
                "Silver",
                "Titanium"
            }},
            {"grassyplateaus", new List<string>
            {
                "Copper",
                "Gold",
                "Lead",
                "Lithium",
                "Quartz",
                "Silver",
                "Titanium"
            }},
            {"mushroomforest", new List<string>
            {
                "Gold",
                "Lead",
                "Lithium",
                "Diamond",
                "Silver",
                "Gold",
                "Copper",
                "Titanium"
            }},
            {"koosh", new List<string>
            {
                "Diamond",
                "Gold",
                "Lead",
                "Lithium",
                "AluminumOxide",
                "Titanium",
                "Silver"
            }},
            {"jellyshroom", new List<string>
            {
                "Gold",
                "Lithium",
                "Magnetite",
                "Nickel",
                "Diamond"
            }},
            {"sparsereef", new List<string>
            {
                "Copper",
                "Lithium",
                "Titanium",
                "Quartz",
                "Gold",
                "Diamond"
            }},
            {"grandreef", new List<string>
            {
                "Diamond",
                "Gold",
                "Lead",
                "Lithium",
                "Quartz",
                "AluminumOxide",
                "UraniniteCrystal",
                "Titanium",
                "Copper",
                "Silver"
            }},
            {"deepgrandreef", new List<string>
            {
                "Diamond",
                "Gold",
                "AluminumOxide"
            }},
            {"dunes", new List<string>
            {
                "Gold",
                "Lead",
                "Lithium",
                "Quartz",
                "Silver",
                "Titanium",
                "Copper",
                "AluminumOxide"
            }},
            {"mountains", new List<string>
            {
                "AluminumOxide",
                "Diamond",
                "Lead",
                "Magnetite",
                "Nickel",
                "UraniniteCrystal",
                "Gold",
                "Lithium",
                "Silver"
            }},
            {"bloodkelp", new List<string>
            {
                "Diamond",
                "Gold",
                "Quartz",
                "UraniniteCrystal",
                "Copper",
                "Lead",
                "AluminumOxide",
                "Lithium"
            }},
            {"underwaterislands", new List<string>
            {
                "Copper",
                "Diamond",
                "Gold",
                "Titanium",
                "UraniniteCrystal",
                "Quartz",
                "Quartz",
                "AluminumOxide",
                "Silver",
                "Lithium",
                "Lead"
            }},
            {"floating", new List<string>
            {
                "Lithium",
                "Diamond"
            }},
            {"lostriver", new List<string>
            {
                "AluminumOxide",
                "Sulphur",
                "UraniniteCrystal",
                "Titanium",
                "Quartz",
                "Nickel",
                "Copper",
                "Lead",
                "Silver",
                "Gold",
                "Lithium",
                "Diamond"
            }},
            {"activelavazone", new List<string>
            {
                "Kyanite",
                "Lead",
                "Sulphur"
            }},
            {"alz", new List<string>
            {
                "Kyanite",
                "Lead",
                "Sulphur"
            }},
            {"inactivelavazone", new List<string>
            {
                "Diamond",
                "Kyanite",
                "AluminumOxide",
                "Sulphur",
                "Titanium",
                "Gold",
                "Lithium",
                "Diamond"
            }},
            {"ilz", new List<string>
            {
                "Diamond",
                "Kyanite",
                "AluminumOxide",
                "Sulphur",
                "Titanium",
                "Gold",
                "Lithium",
                "Diamond"
            }},
            {"cragfield", new List<string>
            {
                "Lithium",
                "Gold",
                "Lithium",
                "Diamond",
                "Quartz",
                "Lead",
                "Silver",
                "Copper",
                "Titanium"
            }},
            {"crashzone", new List<string>
            {
                "Gold",
                "Lead",
                "Lithium",
                "Quartz",
                "Titanium",
                "Silver",
                "Copper"
            }},
            {"ghosttree", new List<string>
            {
                "AluminumOxide",
                "Sulphur",
                "UraniniteCrystal",
                "Titanium",
                "Nickel"
            }},
            {"lava", new List<string>
            {
                "Diamond",
                "Kyanite",
                "Sulphur",
                "Copper",
                "Titanium",
                "Quartz"
            }},
            {"seatreaderpath", new List<string>
            {
                "AluminumOxide",
                "Lithium",
                "Copper",
                "Titanium",
                "Quartz",
                "Gold",
                "Lead",
                "Silver",
                "Diamond"
            }},
            {"void", new List<string>
            {
                "Gold",
                "Lead",
                "Lithium",
                "Quartz",
                "Titanium",
                "Diamond",
                "Kyanite",
                "AluminumOxide",
                "Sulphur",
                "UraniniteCrystal",
                "Silver",
                "Copper",
                "Nickel",
                "Magnetite"
            }},
        };

        [JsonIgnore] internal Dictionary<string, List<TechType>> BiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();

        #endregion
        internal void Convert()
        {
            foreach (KeyValuePair<string, List<string>> biomeOre in BiomeOres)
            {
                var types = new List<TechType>();

                foreach (string sTechType in biomeOre.Value)
                {
                    types.Add(sTechType.ToTechType());
                }
                QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                BiomeOresTechType.Add(biomeOre.Key, types);
            }

            BiomeOres = null;
        }
    }

    public class StorageConfig
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}
