﻿using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using Nautilus.Json.Attributes;
using Nautilus.Json;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

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
                new ColorTemplate{ TemplateName = "Template 6" },
                new ColorTemplate{ TemplateName = "Template 7" },
                new ColorTemplate{ TemplateName = "Template 8" },
                new ColorTemplate{ TemplateName = "Template 9" },
                new ColorTemplate{ TemplateName = "Template 10" },
                new ColorTemplate{ TemplateName = "Template 11" },
                new ColorTemplate{ TemplateName = "Template 12" },
                new ColorTemplate{ TemplateName = "Template 13" },
                new ColorTemplate{ TemplateName = "Template 14" },
                new ColorTemplate{ TemplateName = "Template 15" },
                new ColorTemplate{ TemplateName = "Template 16" },
                new ColorTemplate{ TemplateName = "Template 17" },
                new ColorTemplate{ TemplateName = "Template 18" },
                new ColorTemplate{ TemplateName = "Template 19" },
                new ColorTemplate{ TemplateName = "Template 20" },
                new ColorTemplate{ TemplateName = "Template 21" },
                new ColorTemplate{ TemplateName = "Template 22" },
                new ColorTemplate{ TemplateName = "Template 23" },
                new ColorTemplate{ TemplateName = "Template 24" }
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

public class BaseSaveData : ISaveDataEntry
{
    public string Id { get; set; }
    public string BaseId { get; set; }
    public ColorTemplateSave ColorTemplate { get; set; }
}
