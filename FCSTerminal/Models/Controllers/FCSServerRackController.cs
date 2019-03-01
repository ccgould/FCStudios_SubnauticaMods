using FCSTerminal.Logging;
using ProtoBuf;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSTerminal.Models.Controllers
{
    [ProtoContract]
    public class FCSServerRackController : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener, IHandTarget
    {
        private Constructable _buildable = null;

        private ItemsContainer _container;

        public byte[] _serializedStorage;

        public void Awake()
        {
            if (_buildable == null)
            {
                _buildable = GetComponentInChildren<Constructable>();
            }
        }

        private ItemsContainer container
        {
            get
            {
                if (_container == null)
                {
                    _container = new ItemsContainer(1, 8, gameObject.transform, "FCS Server Rack", null);
                    _container.isAllowedToAdd += IsAllowedToAdd;
                    _container.isAllowedToRemove += IsAllowedToRemove;
                    _container.onAddItem += OnAddItem;
                    _container.onRemoveItem += OnRemoveItem;
                }
                return _container;
            }
        }

        private void OnRemoveItem(InventoryItem item)
        {

            var servers = transform.Find("model").Find("Servers").transform;
            if (servers != null)
            {

                Log.Info(servers.ToString());
                //.GetItems(LoadItems.SERVER_PREFAB_OBJECT.TechType)
                Log.Info("Getting Items");
                var items = transform.GetComponent<FCSServerRackController>()._container;
                Log.Info("Got items");

                if (items.count != 0)
                {
                    List<Transform> childs = servers.Cast<Transform>().ToList();

                    var last = childs.Last(x => x.gameObject.activeSelf);

                    last.gameObject.SetActive(false);
                }
                else
                {
                    Log.Info("There are no items in this rack so i will disable all servers");
                    foreach (Transform server in servers)
                    {
                        server.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Log.Error("Cannot find ServerRack(Clone) to remove the server from");

            }
        }

        private void OnAddItem(InventoryItem item)
        {
            if (item != null) item.isEnabled = item.item.GetTechType().Equals(LoadItems.SERVER_PREFAB_OBJECT.TechType);

            var servers = gameObject.FindChild("model").FindChild("Servers").transform;


            var items = _container.GetItems(LoadItems.SERVER_PREFAB_OBJECT.TechType);

            var slot = items.IndexOf(item);

            //Log.Info($"Items Count {items.Count} || Slot index {slot} || Servers Count {servers.childCount}");

            foreach (Transform server in servers)
            {
                if (server.name.Equals($"Server_{slot + 1}"))
                {
                    server.gameObject.SetActive(true);
                    break;
                }
            }
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            bool flag = true;

            if (pickupable != null) flag = pickupable.GetTechType().Equals(LoadItems.SERVER_PREFAB_OBJECT.TechType);

            if (!flag && verbose)
                ErrorMessage.AddMessage("I have no idea");

            //Log.Info($" Flag = {flag} | verbose = {verbose} Pickupable = {pickupable}");

            return flag;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {

            TechTypeHandler.TryGetModdedTechType("Server", out TechType warpCannonTechType);
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                if (techType.Equals(LoadItems.SERVER_PREFAB_OBJECT.TechType))
                    flag = true;
            }
            if (!flag && verbose)
                ErrorMessage.AddMessage("Server Racks allowed only");
            return flag;
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            //throw new System.NotImplementedException();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            ///throw new System.NotImplementedException();
        }

        public float constructed { get; set; }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            //TODO Comeback here
            //this.container.Clear(false);

            //if (this._serializedStorage != null)
            //{
            //    StorageHelper.RestoreItems(serializer, this._serializedStorage, this.container);
            //    this._serializedStorage = null;
            //}
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText("Open Server Rack", false, HandReticle.Hand.Left);
        }

        public void OnHandClick(GUIHand hand)
        {
            PDA pda = Player.main.GetPDA();
            Inventory.main.SetUsedStorage(container, false);
            pda.Open(PDATab.Inventory, gameObject.transform, null, 4f);
        }
    }
}
