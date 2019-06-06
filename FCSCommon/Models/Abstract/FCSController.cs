using System;
using UnityEngine;

namespace FCSCommon.Models.Abstract
{
    public abstract class FCSController : MonoBehaviour
    {
        public abstract bool IsConstructed { get; }
        public virtual Action OnMonoUpdate { get; set; }

        public virtual void OnAddItemEvent(InventoryItem item) { }

        public virtual void OnRemoveItemEvent(InventoryItem item) { }
    }
}
