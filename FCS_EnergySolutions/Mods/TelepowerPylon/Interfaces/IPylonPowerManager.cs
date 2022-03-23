﻿using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Interfaces
{
    internal interface IPylonPowerManager
    {
        void AddConnection(BaseTelepowerPylonManager controller, bool toggleSelf = false);
    }
}