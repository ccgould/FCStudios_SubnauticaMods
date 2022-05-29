using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    internal class FCSStorageNonSerialized : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener
    {
        public ItemsContainer container { get; private set; }
        public string storageLabel = "VehicleStorageLabel";
        public int width = 4;
        public int height = 4;
        public ChildObjectIdentifier storageRoot;
        private const int currentVersion = 3;
        public int version = 3;


        private void Awake()
        {
            this.Init();
        }

        private void Init()
        {
            if (this.container != null)
            {
                return;
            }
            this.container = new ItemsContainer(this.width, this.height, this.storageRoot.transform, this.storageLabel, null);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }
        
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            StorageHelper.TransferItems(this.storageRoot.gameObject, this.container);
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            StorageHelper.TransferItems(this.storageRoot.gameObject, this.container);
        }
    }
}
