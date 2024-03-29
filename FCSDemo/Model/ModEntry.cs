﻿#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace FCSDemo.Model
{
    internal class ModEntry
    {
        public string PrefabName { get; set; }
        public string FriendlyName { get; set; }
        public string ClassID { get; set; }
        [JsonIgnore]public GameObject Prefab { get; set; }
        public string IconName { get; set; }
    }
}
