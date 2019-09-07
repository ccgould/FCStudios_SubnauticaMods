using FCSCommon.Objects;
using FCSCommon.Utilities;
using HorizontalDuck.Buildables;
using HorizontalDuck.Config;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System.IO;
using UnityEngine;

namespace HorizontalDuck.Mono
{
    internal class HorizontalDuckController : HandTarget, IHandTarget, IProtoEventListener
    {
        private PrefabIdentifier _prefabID;
        private readonly string SaveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "HorizontalDuck");
        private string SaveFile => Path.Combine(SaveDirectory, _prefabID.Id + ".json");

        private void Awake()
        {
            _prefabID = gameObject.GetComponent<PrefabIdentifier>();
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!base.enabled)
                return;

            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            HandReticle.main.SetInteractText(HorizontalDuckBuildable.ResizeMessage());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!base.enabled)
                return;

            if (Input.GetKey(KeyCode.E))
            {
                GameObject model = this.gameObject;

                if (model == null)
                    return;

                BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();
                if (model.transform.localScale.y > 10.0f)
                {
                    model.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    collider.size = new Vector3(0.1650754f, 0.2141044f, 0.2005115f);
                }
                else
                {
                    model.transform.localScale *= 1.25f;
                    collider.size *= 0.5f;
                }
            }
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Info($"Saving {_prefabID.Id} Data");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            BoxCollider collider = gameObject.GetComponent<BoxCollider>();

            var saveData = new SaveData
            {
                Size = new Vec3(collider.size.x, collider.size.y, collider.size.z),
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Info($"Saved {_prefabID.Id} Data");
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabID != null)
            {
                QuickLogger.Info($"Loading Horizontal Duck {_prefabID.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                    BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();
                    collider.size = new Vector3(savedData.Size.X, savedData.Size.Y, savedData.Size.Z);
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
    }
}
