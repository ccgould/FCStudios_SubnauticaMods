using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Services;
using FCSCommon.Utilities;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FCS_EnergySolutions.Configuration;

[Menu("FCS Energy Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("energySolutions-config", "Configurations") { }

    [Toggle("[Energy Solutions] Enable Debugs", Order = 0, Tooltip = "Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)"), OnChange(nameof(EnableDebugsToggleEvent))]
    public bool EnableDebugLogs = false;

    [Slider("[PowerStorage] Power Drain Per Second", 1, 50, Step = 0.1f, Format = "{0:F2}", DefaultValue = 1,
    Order = 1, Tooltip = "Allows you to adjust the power drain of powerstorage. (The higher the faster Power Storage will charge.")]
    public float PowerStoragePowerDrainPerSecond = 1f;

    [Slider("[Telepower Pylon] Pylon Effects Brightness", 0, 1, Step = 0.1f, Format = "{0:F2}", DefaultValue = 1,
    Order = 1, Tooltip = "Allows you to adjust the brightness of the trail effect in the telepower Pylon effects."),
    OnChange(nameof(BrightnessChangeEvent))]

    internal Dictionary<string, float> JetStreamT242BiomeSpeeds { get; set; } = new Dictionary<string, float>
        {
            {"kelpforest",283f },
            {"mushroomforest",200f },
            {"koosh",220f },
            {"jellyshroom",166f },
            {"sparsereef",250f },
            {"grandreef",300f },
            {"deepgrandreef",295f },
            {"dunes",295f },
            {"mountains",275f },
            {"bloodkelp",255f },
            {"underwaterislands",282f },
            {"inactivelavazone",295f },
            {"floaterislands",298f },
            {"lostriver",267f },
            {"ghosttree",267f },
            {"skeletoncave",267f },
            {"activelavazone",300f },
            {"crashzone",300f },
            {"seatreaderpath",300f },
            {"safe",300f },
            {"grassy",296f },
            {"crag",275f },
            {"ilz",295f },
            {"alz",300f },
            {"lava",300f },
            {"void",300f },
            {"floating",300f },
            {"None",0f }
        };
    public float TelepowerPylonTrailBrightness { get; set; } = 1;

    private void EnableDebugsToggleEvent(ToggleChangedEventArgs e)
    {
        if (e.Value)
        {
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
        }
        else
        {
            QuickLogger.DebugLogsEnabled = false;
            QuickLogger.Info("Debug logs disabled");
        }
    }

    private static void BrightnessChangeEvent(SliderChangedEventArgs e)
    {
        HabitatService.main.GlobalNotifyByID("PS", "UpdateEffects");
    }
}
