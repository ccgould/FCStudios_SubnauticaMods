using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Enums;
using Nautilus.Json;
using Nautilus.Json.Attributes;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using static FCS_AlterraHub.Core.Services.SaveLoadDataService;

namespace FCS_AlterraHub.Configuation;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
    public GSPSaveData GamePlayService { get; set; } = new();
    public AccountDetails AccountDetails { get;  set; }
    public StoreManagerSaveData StoreManagerSaveData { get; set; }

    public class PDASaveData : BaseSaveData
    {


    }

    [Table("OreConsumer")]
    public class OreConsumerDataEntry : BaseSaveData
    {
        // Serialized queue
        public string OreQueueJson { get; set; }

        [Ignore]
        public Queue<TechType> OreQueue
        {
            get => string.IsNullOrEmpty(OreQueueJson)
                ? new Queue<TechType>()
                : JsonConvert.DeserializeObject<Queue<TechType>>(OreQueueJson);
            set => OreQueueJson = JsonConvert.SerializeObject(value);
        }

        public float TimeLeft { get;  set; }
        public float RPM { get; set; }
        public bool IsBreakerTripped { get;  set; }
        public OreConsumerSpeedModes CurrentSpeedMode { get; set; }
        public OreConsumerSpeedModes PendingSpeedMode { get; set; }
    }



    [FileName("AlterraHub")]
    public class AlterraHubSaveData : SaveDataCache
    {
        public Dictionary<string, BaseRackSaveData> savedRacks = new();
        public List<ColorTemplate> colorTemplate = new List<ColorTemplate>
            {
                new ColorTemplate
                {
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.cyan,
                    TemplateName = "Template 1"
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.red,
                    TemplateName = "Template 2"
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.magenta,
                    TemplateName = "Template 3"
                },
                new ColorTemplate
                {
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.8941177f,0.6784314f,0.01960784f,1f),
                    EmissionColor = Color.cyan,
                    TemplateName = "Template 4"
                },
                new ColorTemplate{ TemplateName = "Template 5" },
            };
        public Dictionary<string, PaintToolDataEntry> paintTools = new();
        public Dictionary<string, BaseInfoSaveData> bases = new();
    }

    public class BaseInfoSaveData
    {
        public string FriendlyName { get; set; }
        public int BaseId { get; set; }
    }

    public class BaseRackSaveData
    {
        public Dictionary<string, int> EquippedTechTypesInSlots;
    }

    public class PaintToolDataEntry
    {
        public int Amount { get; set; }
        public int CurrentTemplateIndex { get; set; }
    }
}


public class BaseSaveData : ISaveDataEntry, IDBEntity
{
    private ColorTemplateSave colorTemplate;

    [Unique]
    [PrimaryKey]
    public string Id { get; set; }
    public string BaseId { get; set; }
    public string ColorTemplateJson { get; set; }

    [Ignore]
    public ColorTemplateSave ColorTemplate {
        get => string.IsNullOrEmpty(ColorTemplateJson)
            ? new ColorTemplateSave()
            : JsonConvert.DeserializeObject<ColorTemplateSave>(ColorTemplateJson);
        set => ColorTemplateJson = JsonConvert.SerializeObject(value);
    }
}
