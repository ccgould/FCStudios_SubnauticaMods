using UnityEngine;

namespace FCS_DeepDriller.Model
{
    internal class Pillar
    {
        public GameObject Root;
        public string Name;
        public float Length;
        public bool IsExtended;

        public Pillar(GameObject go)
        {
            Name = go.name;
            Root = go;
        }
    }
}
