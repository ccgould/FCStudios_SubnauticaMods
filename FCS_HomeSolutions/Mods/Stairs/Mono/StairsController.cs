using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stairs.Mono
{
    class StairsController : FcsDevice, IFCSSave<SaveData>
	{

		private bool _runStartUpOnEnable;
		private bool _isFromSave;
		private StairsDataEntry _savedData;

		public override Vector3 GetPosition()
		{
			return transform.position;
		}

		private void OnEnable()
		{
			if (_runStartUpOnEnable)
			{
				if (!IsInitialized)
				{
					Initialize();
				}

				if (_isFromSave)
				{
					if (_savedData == null)
					{
						ReadySaveData();
					}

					_colorManager.LoadTemplate(_savedData.ColorTemplate);
				}

				_runStartUpOnEnable = false;
			}
		}

		public override void Initialize()
        {

            constructableBounds = gameObject.GetComponent<ConstructableBounds>();

			if (_colorManager == null)
			{
				_colorManager = gameObject.AddComponent<ColorManager>();
				_colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, AlterraHub.BaseLightsEmissiveController);

			}

			MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
			MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 3f);
			IsInitialized = true;
		}

		public override void OnProtoSerialize(ProtobufSerializer serializer)
		{
			QuickLogger.Debug("In OnProtoSerialize");

			if (!Mod.IsSaving())
			{
				QuickLogger.Info($"Saving {GetPrefabID()}");
				Mod.Save(serializer);
				QuickLogger.Info($"Saved {GetPrefabID()}");
			}
		}

		public override void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			QuickLogger.Debug("In OnProtoDeserialize");

			if (_savedData == null)
			{
				ReadySaveData();
			}

			_isFromSave = true;
		}

		public void Save(SaveData newSaveData, ProtobufSerializer serializer)
		{
			if (!IsInitialized
				|| !IsConstructed) return;

			if (_savedData == null)
			{
				_savedData = new StairsDataEntry();
			}

			_savedData.Id = GetPrefabID();
			_savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.ChildCount = _childCount;
            _savedData.FloorDistance = _cachedFloorDistance;
            _savedData.FloorDistance2 = _cachedFloorDistance2;
			QuickLogger.Debug($"Saving ID {_savedData.Id}");
			newSaveData.StairsEntries.Add(_savedData);
		}

		private void ReadySaveData()
		{
			QuickLogger.Debug("In OnProtoDeserialize");
			_savedData = Mod.GetStairsEntrySaveData(GetPrefabID());
		}

		public override bool CanDeconstruct(out string reason)
		{
			reason = string.Empty;
			return true;
		}

		public override void OnConstructedChanged(bool constructed)
		{
			IsConstructed = constructed;
			if (constructed)
			{
				if (isActiveAndEnabled)
				{
					if (!IsInitialized)
					{
						Initialize();
					}

					IsInitialized = true;
				}
				else
				{
					_runStartUpOnEnable = true;
				}
			}
		}

		public override bool ChangeBodyColor(ColorTemplate template)
		{
			return _colorManager.ChangeColor(template);
		}
	

	    public void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DecorationItemTabId, Mod.ModPackID);
			traceStart = GameObjectHelpers.FindGameObject(gameObject, "RayCast_Point").transform;

            stepPrefab = ModelPrefab.GetPrefab("FCS_DoorStairsExtension_Small");
            
			if(_savedData != null && !Mathf.Approximately(_savedData.FloorDistance,-1))
            {
                BuildStairsFromSave();
            }
            else
            {
                BuildGeometry(Manager?.BaseComponent, false);
			}
		}

        private void BuildStairsFromSave()
        {
            BuildGeometry(Manager?.BaseComponent,false,_savedData.FloorDistance,_savedData.FloorDistance2);
        }

        private void BuildGeometry(Base baseComp, bool disableColliders)
		{
            Vector3 a = traceStart.transform.up * -0.15f;
			float floorDistance = GetFloorDistance(a + traceStart.position - traceStart.right * StartOffset, traceStart.forward, maxLength, gameObject);
			float floorDistance2 = GetFloorDistance(a + traceStart.position + traceStart.right * StartOffset, traceStart.forward, maxLength, gameObject);
			BuildGeometry(baseComp, disableColliders, floorDistance, floorDistance2);
        }

        private void Refresh()
        {
            foreach (Transform stair in stairsParent.transform)
            {
                Destroy(stair.gameObject);
            }
            Vector3 a = traceStart.transform.up * -0.15f;
            float floorDistance = GetFloorDistance(a + traceStart.position - traceStart.right * StartOffset, traceStart.forward, maxLength, gameObject);
            float floorDistance2 = GetFloorDistance(a + traceStart.position + traceStart.right * StartOffset, traceStart.forward, maxLength, gameObject);
			BuildStairsFromDistance(Manager?.BaseComponent,false,floorDistance,floorDistance2,a);
		}

        private void BuildGeometry(Base baseComp, bool disableColliders, float floorDistance, float floorDistance2)
        {
            if (stairsParent)
            {
                cachedSteps.Clear();
                foreach (Transform child in stairsParent.transform)
                {
                    cachedSteps.Add(child.gameObject);
                }
            }
            stairsParent = new GameObject();
            stairsParent.transform.parent = transform;

            Vector3 a = traceStart.transform.up * -0.15f;

            BuildStairsFromDistance(baseComp, disableColliders, floorDistance, floorDistance2, a);
        }

        private void BuildStairsFromDistance(Base baseComp, bool disableColliders, float floorDistance, float floorDistance2, Vector3 a)
        {
            float num;
            float num2;
            if (floorDistance > floorDistance2)
            {
                num = floorDistance;
                num2 = floorDistance2;
            }
            else
            {
                num = floorDistance2;
                num2 = floorDistance;
            }

            _cachedFloorDistance = floorDistance;
            _cachedFloorDistance2 = floorDistance2;

			if (num > 0f)
            {
                BuildStairs(num, baseComp?.isGhost ?? false, disableColliders);
                constructableBounds.bounds.extents = new Vector3(1.4f, 0.01f, (num2 - 0.5f) / 2f);
                constructableBounds.bounds.position = a + new Vector3(0f, 0.01f, (num2 - 0.5f) / 2f);
                return;
            }

            constructableBounds.bounds.extents = Vector3.zero;
            constructableBounds.bounds.position = Vector3.zero;
        }

        private GameObject CreateStep()
		{
			if (cachedSteps.Count > 0)
			{
				GameObject gameObject = cachedSteps[0];
				cachedSteps.Remove(gameObject);
				return gameObject;
			}
			return Instantiate(stepPrefab, stairsParent.transform);
		}

		private void BuildStairs(float length, bool isGhost, bool disableColliders,bool calculateAmount = true)
        {
            int num;
            if (calculateAmount)
            {
                num = Mathf.RoundToInt((length / stepLength + 1.5f) - 1);
			}
            else
            {
                num = (int)length;
            }
            
            for (int i = 0; i < num; i++)
			{
				GameObject step = CreateStep();
				step.transform.position = traceStart.position + (float)i * stepLength * traceStart.forward;
				step.transform.rotation = transform.rotation;
                step.SetActive(true);
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, step, Color.cyan);
                MaterialHelpers.ChangeMaterialColor(AlterraHub.BasePrimaryCol, step, _colorManager.GetColor(ColorTargetMode.Primary));
                _childCount++;
			}

			Collider[] componentsInChildren = stairsParent.GetComponentsInChildren<Collider>();
            for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = !disableColliders;
			}
		}

		public Transform traceStart;

		public GameObject stepPrefab;

		public ConstructableBounds constructableBounds;

		private const float stepLength = 0.752f; //1.671628f;

		private const float maxLength = 10f;

		private GameObject stairsParent;

        private float StartOffset = 0.7f;

		private static List<GameObject> cachedSteps = new();
        private int _childCount;
        private float _cachedFloorDistance;
        private float _cachedFloorDistance2;

        private static bool IsDestroyed(GameObject gameObject)
		{
			return gameObject.tag == "ToDestroy";
		}

		private static bool IsOtherBasePiece(GameObject other, GameObject ignoreObject)
		{
			return (other.GetComponent<Base>() != null || other.GetComponentInParent<Base>() != null); // && !UWE.Utils.SharingHierarchy(other, ignoreObject);
		}
		
        public static bool IsBaseGhost(GameObject obj)
		{
			Transform transform = obj.transform;
			while (transform)
			{
				Base component = transform.GetComponent<Base>();
				if (component)
				{
					return component.isGhost;
				}
				transform = transform.parent;
			}
			return false;
		}

		public static float GetFloorDistance(Vector3 startPoint, Vector3 direction, float maxLength, GameObject ignoreObject = null)
		{
			float num = -1f;
			float num2 = -1f;
			bool flag = false;
			bool flag2 = false;
			int num3 = UWE.Utils.RaycastIntoSharedBuffer(startPoint, direction, maxLength, -1, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < num3; i++)
			{
				RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
				if (raycastHit.collider.gameObject.layer != LayerID.Player && !IsBaseGhost(raycastHit.collider.gameObject) && !IsDestroyed(raycastHit.collider.gameObject))
				{
					if (IsOtherBasePiece(raycastHit.collider.gameObject, ignoreObject) && QPatch.Configuration.StairsLimitLengthOverBasePieces)
					{
						if (num == -1f || num > raycastHit.distance)
						{
							num = raycastHit.distance;
						}
						flag = true;
					}
					else
					{
						if (num2 == -1f || num2 > raycastHit.distance)
						{
							num2 = raycastHit.distance;
						}
						flag2 = true;
					}
				}
			}
			if (flag2 && flag && num2 - num > 0.3f)
			{
				return -1f;
			}
			return num2;
		}
	}
}
