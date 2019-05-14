using UnityEngine;

namespace FCSDeepHarvester.Models.Controllers
{
    public class DeepHarvesterPlanterController : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener, IHandTarget
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public DeepHarvesterPlanterController()
        {
            //new ItemsContainer(1, 1, transform.transform, "Planter1", plantSound);
        }

        private StorageContainer _storage;

        private Constructable _buildable;

        public StorageContainer StorageContainer
        {
            get
            {
                if (_storage == null)
                {
                    _storage = new StorageContainer();
                    _storage.Resize(2, 3);
                }
                return _storage;

            }
        }

        public void Awake()
        {
            if (_buildable == null)
            {
                _buildable = GetComponentInChildren<Constructable>();
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText("Open Planter", false, HandReticle.Hand.Left);
        }

        public void OnHandClick(GUIHand hand)
        {
            PDA pda = Player.main.GetPDA();
            //Inventory.main.SetUsedStorage(container, false);
            pda.Open(PDATab.Inventory, gameObject.transform, null, 4f);
        }

        #endregion

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            //throw new System.NotImplementedException();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            //throw new System.NotImplementedException();
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}
