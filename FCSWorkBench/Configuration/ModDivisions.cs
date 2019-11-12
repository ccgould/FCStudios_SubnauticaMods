using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Mono;

namespace FCSTechFabricator
{
    internal static class ModDivisions
    {
        public static ModKey AlterraMedicalSolutions => new ModKey{Key = "Alterra Medical Solutions",ParentKey="AMS" };
        public static ModKey AlterraElectric => new ModKey { Key = "Alterra Electric", ParentKey = "AE" };
        public static ModKey AlterraIndustrialSolutions => new ModKey { Key = "Alterra Industrial Solutions", ParentKey = "AIS" };
        public static ModKey AlterraRefrigerationSolutions => new ModKey { Key = "Alterra Refrigeration Solutions", ParentKey = "ARS" };
        public static ModKey AlterraShippingSolutions => new ModKey { Key = "Alterra Shipping Solutions", ParentKey = "ASS" };
        public static ModKey AlterraStorageSolutions => new ModKey { Key = "Alterra Storage Solutions", ParentKey = "ASTS" };
    }
}
