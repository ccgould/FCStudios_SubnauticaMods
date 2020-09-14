using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCSDemo.Model
{
    internal class ModEntry
    {
        public string PrefabName { get; set; }
        public string FriendlyName { get; set; }
        public string ClassID { get; set; }
        [JsonIgnore]public GameObject Prefab { get; set; }
    }
}
