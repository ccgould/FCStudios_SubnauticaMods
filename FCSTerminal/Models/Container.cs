using FCSTerminal.Helpers;
using System;
using System.IO;
using UnityEngine;

namespace FCSTerminal.Models
{
    public class Container : HandTarget, IHandTarget, IProtoEventListener
    {
        private StorageContainer _storageContainer = null;

        public override void Awake()
        {
            base.Awake();
            this._storageContainer = this.gameObject.GetComponentInChildren<StorageContainer>();
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!enabled)
                return;

            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_storageContainer != null)
                _storageContainer.OnHandClick(hand);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

            
            //var id = GetComponent<PrefabIdentifier>();
            //if (id == null || _storageContainer == null)
            //    return;

            //var saveFolder = FilesHelper.GetSaveFolderPath();

            //if (!Directory.Exists(saveFolder))
            //    Directory.CreateDirectory(saveFolder);

            //GameObject model = gameObject.FindChild("server_rack");

            //string saveData = Convert.ToString(model.transform.localScale.y);
            //BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();
            //saveData += Environment.NewLine + Convert.ToString(collider.size.x) + "|" + Convert.ToString(collider.size.y) + "|" + Convert.ToString(collider.size.z);

            //File.WriteAllText(Path.Combine(saveFolder, "server_rack_" + id.Id + ".txt"), saveData);
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
           
        }
    }
}
