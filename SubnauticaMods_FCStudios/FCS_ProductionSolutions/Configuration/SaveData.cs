using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_ProductionSolutions.Configuration
{
    internal class SaveData
    {
        [JsonProperty]
        public HashSet<object> Data { get; set; } = new ();

        public class CubeGeneratorSaveData : ISaveDataEntry
        {
            public string Id { get; set; }
            public string BaseId { get; set; }
            public ColorTemplateSave ColorTemplate { get; set; }
            public IList<float> Progress { get; set; }
            public float StartUpProgress { get; set; }
            public float GenerationProgress { get; set; }
            public float CoolDownProgress { get; set; }
            public IonCubeGenSpeedModes CurrentSpeedMode { get; set; }
            public int NumberOfCubes { get; set; }
        }
    }
}
