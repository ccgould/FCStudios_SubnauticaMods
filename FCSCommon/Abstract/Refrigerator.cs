using FCSCommon.Controllers;
using FCSCommon.Interfaces;
using UnityEngine;

namespace FCSCommon.Abstract
{
    public abstract class Refrigerator : MonoBehaviour, IRenameNameTarget
    {
        public NameController NameController { get; set; }
    }
}
