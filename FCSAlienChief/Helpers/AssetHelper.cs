using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FCSAlienChief.Data
{
    public static class AssetHelper
    {
        public static AssetBundle Asset = AssetBundle.LoadFromFile($"{Environment.CurrentDirectory}/QMods/FCSAlienChief/fcsalienchief-mod");
    }
}
