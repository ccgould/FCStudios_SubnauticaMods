namespace FlexibleTechFabricator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using QModManager.API.ModLoading;

    // TOOD - create mod.json
    [QModCore]
    public static class Initializer
    {
        [QModPostPatch]
        public static void PatchFabricator()
        {
            FcTechFabricatorService.InternalAPI.PatchFabricator();
        }
    }
}
