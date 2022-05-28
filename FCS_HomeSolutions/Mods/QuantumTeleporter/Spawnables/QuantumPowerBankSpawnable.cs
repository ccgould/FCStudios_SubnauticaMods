using System;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Spawnables
{
    internal class QuantumPowerBankSpawnable : Spawnable
    {
        private readonly GameObject _prefab;
        public override string AssetsFolder => Mod.GetAssetPath();
        public static TechType PatchedTechType { get; private set; }
        public QuantumPowerBankSpawnable() : base("QuantumPowerBank", "Quantum Power Bank", "Energy source for away-from-base Quantum Teleportation via the FCStudios PDA. Holds enough energy for 2 personal or 1 vehicle Teleport.")
        {
            _prefab = ModelPrefab.GetPrefab("FCS_QuantumPowerBank");
            OnFinishedPatching += () =>
            {
                // Add the new TechType to Hand Equipment type.
                SMLHelper.V2.Handlers.CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.Hand);

                // Set quick slot type.
                SMLHelper.V2.Handlers.CraftDataHandler.SetQuickSlotType(this.TechType, QuickSlotType.Selectable);
                
                PatchedTechType = TechType;

                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, TechType, 1,650000, StoreCategory.Home,true);

                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };


        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);
                
                prefab.AddComponent<PrefabIdentifier>();
                prefab.AddComponent<TechTag>().type = TechType;

                var rigidBody = prefab.EnsureComponent<Rigidbody>();

                var pickUp = prefab.AddComponent<Pickupable>();
                pickUp.randomizeRotationWhenDropped = true;
                pickUp.isPickupable = true;
                
                // Make the object drop slowly in water
                var wf = prefab.AddComponent<WorldForces>();
                wf.underwaterGravity = 0;
                wf.underwaterDrag = 10f;
                wf.enabled = true;
                wf.useRigidbody = rigidBody;
                
                //Renderer
                var renderer = prefab.GetComponentInChildren<Renderer>();

                GameObjectHelpers.SetDefaultSkyApplier(prefab);

                var placeTool = prefab.AddComponent<PlaceTool>();
                placeTool.allowedInBase = true;
                placeTool.allowedOnBase = false;
                placeTool.allowedOnCeiling = false;
                placeTool.allowedOnConstructable = true;
                placeTool.allowedOnGround = true;
                placeTool.allowedOnRigidBody = true;
                placeTool.allowedOnWalls = false;
                placeTool.allowedOutside = true;
                placeTool.rotationEnabled = true;
                placeTool.enabled = true;
                placeTool.hasAnimations = false;
                placeTool.hasBashAnimation = false;
                placeTool.hasFirstUseAnimation = false;
                placeTool.mainCollider = prefab.GetComponentInChildren<Collider>();
                placeTool.pickupable = pickUp;
                placeTool.drawTime = 0.5f;
                placeTool.dropTime = 1;
                placeTool.holsterTime = 0.35f;

                // Set large world entity
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

                //var coi = prefab.EnsureComponent<ChildObjectIdentifier>();

                //prefab.gameObject.SetActive(false);
                //var energyMixin = prefab.EnsureComponent<EnergyMixin>();
                //energyMixin.storageRoot = coi;
                //energyMixin.allowBatteryReplacement = false;
                //prefab.gameObject.SetActive(true);
                
                prefab.AddComponent<QuantumPowerBankController>();

                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.red);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }

        internal void OnEventListen()
        {
            QuickLogger.Debug("OnEventListen Triggered", true);
            FCSAlterraHubService.PublicAPI.TeleportationIgnitiated += manager =>
            {
                if (Player.main.IsPiloting())
                {
                    foreach (var fcsDevice in manager.GetDevices(QuantumTeleporterVehiclePadBuildable.QuantumTeleporterVehiclePadTabID))
                    {
                        if (fcsDevice != null && fcsDevice is QuantumTeleporterVehiclePadController controller)
                        {
                            
                            if (controller.IsOperational)
                            {
                                if (Teleport(controller.UnitID))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {

                    foreach (var fcsDevice in manager.GetDevices(QuantumTeleporterBuildable.QuantumTeleporterTabID))
                    {
                        if (fcsDevice != null && fcsDevice is QuantumTeleporterController controller)
                        {
                            if (controller.IsOperational)
                            {
                                if (Teleport(controller.UnitID))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            };
        }
        
        private bool Teleport(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var device = FCSAlterraHubService.PublicAPI.FindDevice(id);

                if (device.Value != null)
                {
                    var destinationTeleporter = device.Value.gameObject.GetComponent<IQuantumTeleporter>();

                    if (!destinationTeleporter.IsOperational)
                    {
                        QuickLogger.ModMessage("This teleporter is not Operational.");
                    }

                    if (device.Value.Manager.IsSame(Player.main.GetCurrentSub()))
                    {
                        QuickLogger.ModMessage("Cannot teleport to the same base.");
                    }

                    var powerBankTechType = "QuantumPowerBank".ToTechType();

                    if (PlayerInteractionHelper.HasItem(powerBankTechType))
                    {
                        var powerBanks = PlayerInteractionHelper.GetItemsOnPlayer(powerBankTechType);

                        foreach (InventoryItem bank in powerBanks)
                        {
                            //Get power bank controller
                            var bankController = bank.item.gameObject.GetComponent<QuantumPowerBankController>();

                            //Check if the power bank has enough power
                            if (!bankController.PowerManager.HasEnoughPower(Player.main.IsPiloting() ? QTTeleportTypes.Vehicle : QTTeleportTypes.Global)) continue;

                            //Check if a valid destination
                            if (!ValidateDestination(destinationTeleporter, out string result))
                            {
                                QuickLogger.ModMessage(result);
                                return false;
                            }

                            FCSPDAController.Main.GoToPage(PDAPages.Home);
                            FCSPDAController.Main.Close();
                            TeleportManager.TeleportPlayer(bankController, destinationTeleporter, Player.main.IsPiloting() ? QTTeleportTypes.Vehicle : QTTeleportTypes.Global);
                            QuickLogger.ModMessage("Teleport SuccessFull");
                            return true;
                        }

                        QuickLogger.ModMessage("Power bank doesn't have enough power for teleporting");
                        return false;
                    }

                    QuickLogger.ModMessage("Requires a Quantum Power Bank on person");
                    return false;
                }

                QuickLogger.ModMessage($"Failed to find teleporter with the ID of : {id}");
                return false;
            }

            return false;
        }

        public static bool ValidateDestination(IQuantumTeleporter destinationTeleporter, out string result)
        {
            result = string.Empty;

            //Check is player is in a vehicle. If so make sure the destination is not in a base
            if (Player.main.IsPiloting())
            {
                if (destinationTeleporter.IsInside())
                {
                    result = "Cant teleport a vehicle to this teleporter";
                    return false;
                }
            }

            //Check if destination is operational
            if (!destinationTeleporter.IsOperational)
            {
                result = "Destination Teleporter is not operational";
                return false;
            }

            //Check if there is enough power at destination
            if (!destinationTeleporter.PowerManager.HasEnoughPower(QTTeleportTypes.Global))
            {
                result = "Target destination doesn't have enough power to teleport";
                return false;
            }

            return true;
        }

    }

    internal class QuantumPowerBankController : MonoBehaviour, IQuantumTeleporter, IProtoEventListener
    {
        private string _prefabID;
        private QuantumPowerBankDataEntry _savedData;
        private bool _isFromSave;
        public BaseManager Manager { get; set; }
        public IQTPower PowerManager { get; set; }

        private void Awake()
        {
            PowerManager = new QuantumPowerBankPowerManager(this);
        }

        public bool IsOperational => true;

        public Transform GetTarget(TeleportItemType senderType, string senderID)
        {
            return transform;
        }
        
        public bool IsInside()
        {
            return Player.main.IsInside();
        }

        private void OnEnable()
        {
            GetPrefabID();

            Mod.RegisterQuantumPowerBank(this);

            if (_isFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                PowerManager.SetCharge(_savedData?.Charge ?? 0f);
                _isFromSave = false;
            }
        }
        
        private void OnDestroy()
        {
            Mod.UnRegisterQuantumPowerBank(this);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetQuantumPowerBankEntrySaveData(GetPrefabID());
        }


        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Paint Tool Save Entry{GetPrefabID()}", true);

            if (_savedData == null)
            {
                _savedData = new QuantumPowerBankDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Charge = PowerManager.PowerAvailable();

            newSaveData.QuantumPowerBankEntries.Add(_savedData);
        }

        public string GetPrefabID()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                _prefabID = gameObject.GetComponent<PrefabIdentifier>()?.Id ??
                            gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
            }

            return _prefabID;
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _isFromSave = true;
        }
    }


    internal class QuantumPowerBankPowerManager : IQTPower, IBattery
    {
        private readonly QuantumPowerBankController _controller;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);

        public QuantumPowerBankPowerManager(QuantumPowerBankController controller)
        {
            _controller = controller;
            UpdateColor();
        }

        public float charge { get; set; } = 3000;
        public float capacity { get; set; } = 3000;

        public string GetChargeValueText()
        {
            return $"Power: {charge}/{capacity}";
        }
        
        public bool TakePower(QTTeleportTypes tab)
        {
            switch (tab)
            {
                case QTTeleportTypes.Global:
                    ModifyCharge(-1500);
                    return true;
                case QTTeleportTypes.Vehicle:
                    ModifyCharge(-3000);
                    return true;
                default:
                    return false;
            }
        }

        public void ModifyCharge(float amount)
        {
            charge = Mathf.Clamp(charge + amount,0,capacity);
            UpdateColor();
        }

        public bool IsFull()
        {
            return charge >= capacity;
        }

        public bool HasEnoughPower(QTTeleportTypes selectedTab)
        {
            switch (selectedTab)
            {
                case QTTeleportTypes.Global:
                    return charge >= 1500;
                case QTTeleportTypes.Vehicle:
                    return charge >= 3000;
                default:
                    return false;
            }
        }

        public float PowerAvailable()
        {
            return charge;
        }

        public void FullReCharge()
        {
            charge = 3000;
        }

        public void SetCharge(float charge)
        {
            this.charge = charge;
            UpdateColor();
        }

        private void UpdateColor()
        {
            Color value;

            var percent = charge / capacity;

            if (charge >= 0f)
            {
                value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);

            }
            else
            {
                value = _colorEmpty;
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, _controller.gameObject, value);
        }
    }
}
