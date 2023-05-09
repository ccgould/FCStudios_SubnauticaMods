using FCS_AlterraHub.Models.Structs;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services
{
    internal class GamePlayService : MonoBehaviour
    {
        public static GamePlayService Main;

        private bool _automaticDebitDeduction;
        private float _rate;
        private ShipmentInfo _shipmentInfo;

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Main != null && Main != this)
            {
                Destroy(this);
            }
            else
            {
                Main = this;
            }
        }

        public void SetAutomaticDebitDeduction(bool value)
        {
            _automaticDebitDeduction = value;
        }

        public bool GetAutomaticDebitDeduction()
        {
            return _automaticDebitDeduction;
        }


        public void SetRate(float value)
        {
            _rate = value;
        }

        public float GetRate()
        {
            return _rate;
        }

        internal void LoadFromSave(GSPSaveData service)
        {
            _shipmentInfo = service.PDAShipmentInfo;
            _automaticDebitDeduction = service.AutomaticDebitDeduction;
            _rate = service.Rate;
        }

        internal GSPSaveData Save()
        {
            return new GSPSaveData(this);
        }

        internal void SetShipmentInfo(ShipmentInfo value)
        {
            _shipmentInfo = value;
        }

        internal ShipmentInfo GetShipmentInfo()
        {
            return _shipmentInfo;
        }
    }
}
