using FCSCommon.Interfaces;
using FCSCommon.Models;
using UnityEngine;

namespace FCSCommon.Abstract
{
    public abstract class Refrigerator : MonoBehaviour, IRenameNameTarget
    {
        public NameController NameController { get; set; }
    }
}
