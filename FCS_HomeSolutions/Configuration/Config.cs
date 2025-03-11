using Nautilus.Json;
using Nautilus.Options.Attributes;
using UnityEngine;


namespace FCS_HomeSolutions.Configuration;
[Menu("FCS Home Solutions Menu")]
public class Config : ConfigFile
{
    public Config() : base("homeSolutions-config", "Configurations") { }


    [Keybind("[Universal Charger] Mode Change  Key", Order = 5, Tooltip = "Switches the mode of the Universal Charger from Powercell to Battery or vice versa.")]
    public KeyCode UniversalChargeModeKey = KeyCode.M;

}
