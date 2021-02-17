using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public abstract class BaseOperationObject : MonoBehaviour
    {
        public abstract List<BaseTransferOperation> Operations { get; set; }
        public abstract string GetPrefabId();
    }
}
