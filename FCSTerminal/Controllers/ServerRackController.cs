using System;
using System.Globalization;
using FCSTerminal.Helpers;
using System.IO;
using UnityEngine;

namespace FCSTerminal.Controllers
{
    public class ServerRackController : HandTarget, IHandTarget, IProtoEventListener
    {
        private StorageContainer _storageContainer = null;

        public override void Awake()
        {
            base.Awake();
            this._storageContainer = this.gameObject.GetComponentInChildren<StorageContainer>();
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!base.enabled)
                return;

            if (this._storageContainer != null)
                this._storageContainer.OnHandClick(hand);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!base.enabled)
                return;

            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            HandReticle.main.SetInteractText("AdjustCargoBoxSize");
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            PrefabIdentifier id = GetComponentInParent<PrefabIdentifier>();
            if (id == null || this._storageContainer == null)
                return;
#if DEBUG_CARGO_CRATES
            Logger.Log("DEBUG: Serialize(): PrefabID=[" + id.Id + "]");
#endif

            string saveFolder = FilesHelper.GetSaveFolderPath();
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            GameObject model = this.gameObject.FindChild("m");
            if (model == null)
                if ((model = this.gameObject.FindChild("cargobox01a")) == null)
                    model = this.gameObject.FindChild("cargobox01b");
            if (model == null)
                return;

            string saveData = Convert.ToString(model.transform.localScale.y);
            BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();
            saveData += Environment.NewLine + Convert.ToString(collider.size.x) + "|" + Convert.ToString(collider.size.y) + "|" + Convert.ToString(collider.size.z);

#if DEBUG_CARGO_CRATES
            Logger.Log("DEBUG: Serialize(): Saving cargo crates nbItems=[" + _storageContainer.container.count + "] size=[" + Convert.ToString(model.transform.localScale.y) + "] collider x=[" + Convert.ToString(collider.size.x) + "] y=[" + Convert.ToString(collider.size.y) + "] z=[" + Convert.ToString(collider.size.z) + "].");
#endif
            File.WriteAllText(Path.Combine(saveFolder, "serverrack_" + id.Id + ".txt"), saveData);
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            PrefabIdentifier id = GetComponentInParent<PrefabIdentifier>();
            if (id == null || this._storageContainer == null)
                return;
#if DEBUG_CARGO_CRATES
            Logger.Log("DEBUG: Deserialize(): PrefabID=[" + id.Id + "]");
#endif

            string filePath = Path.Combine(FilesHelper.GetSaveFolderPath(), "serverrack_" + id.Id + ".txt");
            if (File.Exists(filePath))
            {
                string tmpSize = File.ReadAllText(filePath);
                string[] sizes = tmpSize.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (sizes.Length == 2)
                {
                    GameObject model = this.gameObject.FindChild("cargobox01_damaged");
                    if (model == null)
                        if ((model = this.gameObject.FindChild("cargobox01a")) == null)
                            model = this.gameObject.FindChild("cargobox01b");
                    if (model == null)
                        return;

                    BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();

                    float size = float.Parse(sizes[0], CultureInfo.InvariantCulture.NumberFormat);
                    model.transform.localScale = new Vector3(size, size, size);
                    string[] colliderSizes = sizes[1].Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (colliderSizes.Length == 3)
                    {
                        float colliderSizeX = float.Parse(colliderSizes[0], CultureInfo.InvariantCulture.NumberFormat);
                        float colliderSizeY = float.Parse(colliderSizes[1], CultureInfo.InvariantCulture.NumberFormat);
                        float colliderSizeZ = float.Parse(colliderSizes[2], CultureInfo.InvariantCulture.NumberFormat);
                        collider.size = new Vector3(colliderSizeX, colliderSizeY, colliderSizeZ);
                    }
                    else // Backward compatibility
                    {
                        float colliderSize;
                        if (float.TryParse(sizes[1], out colliderSize))
                            collider.size = new Vector3(colliderSize * 0.4583f, colliderSize, colliderSize * 0.5555f);
                    }
                }
            }
        }
    }
}
