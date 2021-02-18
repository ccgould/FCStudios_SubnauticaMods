using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Transceiver
{
    internal class DSSTransceiverController : BaseOperationObject
    {
        private Rigidbody _rb;
        private BoxCollider[] _colliders;
        private string _prefabId;

        public override string GetPrefabId()
        {
            if (string.IsNullOrWhiteSpace(_prefabId))
            {
                _prefabId = gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                            gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
            }

            return _prefabId;
        }

        public void DockTransceiver(DSSSlotController slot, IDSSRack controller)
        {
            if (_colliders == null)
            {
                _colliders = GetComponentsInChildren<BoxCollider>();
            }
            if (_rb == null)
            {
                _rb = GetComponentInChildren<Rigidbody>();
            }
            _rb.isKinematic = true;
            gameObject.SetActive(true);
            foreach (BoxCollider bc in _colliders)
            {
                bc.isTrigger = true;
            }
            ModelPrefab.ApplyShaders(gameObject);
            gameObject.SetActive(true);
            transform.parent = slot.transform;
            transform.localPosition = Vector3.zero;
        }

        public void UnDockTransceiver(DSSSlotController slot, IDSSRack controller)
        {

        }
    }
}
