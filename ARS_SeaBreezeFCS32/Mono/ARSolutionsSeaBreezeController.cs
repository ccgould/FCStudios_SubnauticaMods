using ARS_SeaBreezeFCS32.Interfaces;
using ARS_SeaBreezeFCS32.Model;
using FCSCommon.Utilities;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal partial class ARSolutionsSeaBreezeController : IFridgeContainer, IProtoTreeEventListener
    {
        private Constructable _buildable;

        private ARSolutionsSeaBreezeContainer _fridgeContainer;
        public bool IsFull { get; }
        public int NumberOfItems { get; set; }
        internal bool IsConstructed => _buildable != null && _buildable.constructed;


        private void Update()
        {

        }

        private void Awake()
        {
            if (FindComponents())
            {
                if (_buildable == null)
                {
                    _buildable = GetComponentInParent<Constructable>();
                }

                _fridgeContainer = new ARSolutionsSeaBreezeContainer(this);
            }
            else
            {
                throw new MissingComponentException("Failed to find all components");
            }
        }

        public void OpenStorage()
        {
            _fridgeContainer.OpenStorage();
        }

        private void Start()
        {

        }

        private bool FindComponents()
        {
            QuickLogger.Debug("********************************************** In FindComponents **********************************************");


            //_turbine = transform.Find("model").Find("anim_parts").Find("Rotor").Find("Turbine_BladeWheel")?.gameObject;

            //if (_turbine == null)
            //{
            //    QuickLogger.Error($"Turbine_BladeWheel not found");
            //    _initialized = false;
            //    return false;
            //}


            return true;
        }

        public void OnAddItemEvent(InventoryItem item)
        {
            _buildable.deconstructionAllowed = false;
        }

        public void OnRemoveItemEvent(InventoryItem item)
        {
            _buildable.deconstructionAllowed = _fridgeContainer.NumberOfItems == 0;
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {

        }
    }
}
