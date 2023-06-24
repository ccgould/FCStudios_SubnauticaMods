using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using Nautilus.Json.Attributes;
using Nautilus.Json;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Configuation;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();


    public GSPSaveData GamePlayService { get; set; } = new();
    public AccountDetails AccountDetails { get;  set; }
    public StoreManagerSaveData StoreManagerSaveData { get; set; }

    public class PDASaveData : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }

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
                    EmissionColor = Color.cyan
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.red
                },
                new ColorTemplate
                {
                    PrimaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    SecondaryColor = new Color(0.1882353f, 0.1843137f, 0.1803922f, 1f),
                    EmissionColor = Color.magenta
                },
                new ColorTemplate
                {
                    PrimaryColor = Color.white,
                    SecondaryColor = new Color(0.8941177f,0.6784314f,0.01960784f,1f),
                    EmissionColor = Color.cyan
                },
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
                new ColorTemplate(),
            };
        public Dictionary<string, PaintToolDataEntry> paintTools = new();
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
