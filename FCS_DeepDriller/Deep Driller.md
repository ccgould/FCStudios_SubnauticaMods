# **ALTERRA DEEP DRILLER CONFIGURATION**

Customization of the deep driller is fairly easy, it requires little knowledge of JSON ([Json Help](https://www.json.org/json-en.html)) to add ores to a biome or change parameters in the deep driller. There are json editors online that can help you visualize the json file ex.([JsonEditorOnline](https://jsoneditoronline.org/)).
![enter image description here](https://i.postimg.cc/52mKg8vG/Deep-Driller.png)
The Config.json file is located in the mod directory and is generated when the game has started. It will not be replaced if it is already created so if there is a new feature in the config we would ask you to delete the file and let the config rebuild.

![enter image description here](https://i.postimg.cc/dVFSG57r/config.png)

There are a few parameters in the config some are explanatory.

- StorageSize : The width and height of the storage container in the drill
- PowerDraw: The amount of power the drill pulls during operation.
- SolarCapacity: The amount of power the solar panel attachment stores.
- DrillOrePerDay:  The base amount of ores the deep driller will drill per day.
- Mk1OrePerDay: The amount of ores the deep driller will drill per day if the MK1 upgrade is installed.
- Mk2OrePerDay: The amount of ores the deep driller will drill per day if the MK2 upgrade is installed.
- Mk3OrePerDay: The amount of ores the deep driller will drill per day if the MK3 upgrade is installed.
- AllowDamage: If set to true  the drill will damage overtime during operation (Also can be found in the menu).
![enter image description here](https://i.postimg.cc/Ls6Cbh3s/Menu.png)


- AdditionalBiomeOres: Allows you to add additional ores to the biome (This is a dictionary of string and list of techtype) if the ore is not in the allowed ores the drill won&#39;t show it as an option.

![enter image description here](https://i.postimg.cc/m2C5cvrT/config-Close.png)
![enter image description here](https://i.postimg.cc/3wSS57tF/Drill-Close.jpg)

**Here is a list of the allowed TechTypes in the drill (Please use the techtype in the config):**

| Allowed TechTypes | Friendly Name |
| --- | --- |
| AluminumOxide | Ruby |
| Sulphur | Sulphur |
| Diamond | Diamond |
| Kyanite | Kyanite |
| Lead | Lead |
| Lithium | Lithium |
| Magnetite | Magnetite |
| Nickel | Nickel |
| Quartz | Quartz |
| Silver | Silver |
| UraniniteCrystal | Uranium |
| Salt | Salt |
| Titanium | Titanium |
| Copper | Copper |
| Gold | Gold |
| LimestoneChunk | Limestone Outcrop |
| ShaleChunk | Shale Outcrop |
| SandstoneChunk | Sandstone Outcrop |
| DrillableSalt | Salt Resource |
| DrillableQuartz | Quartz Resource |
| DrillableCopper | Copper Resource |
| DrillableTitanium | Titanium Resource |
| DrillableLead | Lead Resource |
| DrillableSilver | Silver Resource |
| DrillableDiamond | Diamond Resource |
| DrillableGold | Gold Resource |
| DrillableMagnetite | Magnetite Resource |
| DrillableLithium | Lithium Resource |
| DrillableMercury | Mercury Resource |
| DrillableUranium | Uranium Resource |
| DrillableAluminiumOxide | Ruby Resource |
| DrillableNickel | Nickel Resource |
| DrillableSulphur | Sulphur Resource |
| DrillableKyanite | Kyanite Resource |

