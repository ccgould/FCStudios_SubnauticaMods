using System.Collections.Generic;
using FCSCommon.Utilities;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace FCS_EnergySolutions.Configuration
{
    [Menu("FCS Energy Solutions Menu")]
    internal class Config : ConfigFile
    {
        [Toggle("Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

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
    }
    
    [Menu("JetStream T242 Menu")]
    public class JetStreamT242Config : ConfigFile
    {
        public JetStreamT242Config() : base("jetstreamt242-config", "Configurations")
        {
        }

        public Dictionary<string, float> BiomeSpeeds { get; set; } = new Dictionary<string, float>
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

        public float PowerPerSecond { get; set; } = 0.8333333f;
    }
}
