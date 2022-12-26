using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Enums;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Spawnable;

internal class AlterraHubLifePodDronePortController : MonoBehaviour, IDroneDestination
{
    private Animator _animator;
    public Transform BaseTransform { get; set; }
    public string BaseId { get; set; }
    public bool IsOccupied { get; }


    private List<Transform> _paths;
    private int DOORSTATEHASH;
    private int _portID;
    private DroneController _assignedDrone;
    private GameObject _spawnPoint;
    private GameObject _entryPoint;
    private string _baseID;
    public static AlterraHubLifePodDronePortController main;
    public PortManager PortManager;
    private bool _isPlaying;
    private int _stateHash;


    private void Awake()
    {
        _stateHash = Animator.StringToHash("State");
        main = this;
        var prefabIdent = gameObject.GetComponent<PrefabIdentifier>().id;
        _baseID = BaseManager.FindManager(prefabIdent)?.BaseID;
        DOORSTATEHASH = Animator.StringToHash("DronePortDoorState");
        _animator = gameObject.GetComponentInChildren<Animator>();
        _paths = GameObjectHelpers.FindGameObject(gameObject, "DronePort_DockingPaths").GetChildrenT().ToList();
        _entryPoint = _paths[0].gameObject;
        PortManager.RegisterDronePort(this);
        _spawnPoint = GameObjectHelpers.FindGameObject(gameObject, "SpawnPoint");
    }

    internal PortManager GetDronePortController()
    {
        return PortManager;
    }

    public List<Transform> GetPaths()
    {
        return _paths;
    }

    public void Offload(DroneController order)
    {

    }

    public Transform GetDockingPosition()
    {
        return _paths.Last();
    }

    public void OpenDoors()
    {
        _animator.SetBool(DOORSTATEHASH,true);
    }

    public void CloseDoors()
    {
        _animator.SetBool(DOORSTATEHASH, false);
    }

    public string GetBaseName()
    {
        return "Alterra Hub Pod";
    }

    public void Depart(DroneController droneController)
    {
        SetDockedDrone(null);
    }

    public void Dock(DroneController droneController)
    {
        SetDockedDrone(droneController);
    }

    public int GetPortID()
    {
        return _portID;
    }

    public string GetPrefabID()
    {
        return gameObject.GetComponent<PrefabIdentifier>()?.Id ??
               gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
    }

#if SUBNAUTICA_STABLE
    public DroneController SpawnDrone()
    {
        //TODO V2 Fix
        //    var drone = Instantiate(CraftData.GetPrefabForTechType(Mod.AlterraTransportDroneTechType), _spawnPoint.transform.position, _spawnPoint.transform.rotation);
        //    _assignedDrone = drone.GetComponent<DroneController>();
        return _assignedDrone;
    }
#endif

    public IEnumerator SpawnDrone(Action<DroneController> callback)
    {
        QuickLogger.Info("Spawn Drone");
        var task = CraftData.GetPrefabForTechTypeAsync(Mod.AlterraTransportDroneTechType, false);
        yield return task;

        QuickLogger.Info("Drone Prefab");
        var prefab = task.GetResult();
        if (prefab != null)
        {
            _assignedDrone = Instantiate(prefab).GetComponent<DroneController>();
            QuickLogger.Info("Invoke Spawn Drone");
            callback?.Invoke(_assignedDrone);
        }
    }


    public Transform GetEntryPoint()
    {
        return _entryPoint == null ? null : _entryPoint?.transform;
    }

    public string GetBaseID()
    {
        return _baseID;
    }

    public void SetInboundDrone(DroneController droneController)
    {
            
    }

    public void PlayAnimationState(DronePortAnimation state, Action callBack)
    {
        switch (state)
        {
            case DronePortAnimation.None:
                PlayAnimation(0, "IDLE", callBack);
                CloseDoors();
                break;
            case DronePortAnimation.Docking:
                PlayAnimation(2, "Drone_Approaching", callBack);
                OpenDoors();
                break;
            case DronePortAnimation.Departing:
                PlayAnimation(1, "Drone_Departing", callBack);
                OpenDoors();
                break;
            default:
                PlayAnimation(0, "IDLE", callBack);
                CloseDoors();
                break;
        }
    }

    public void ClearInbound()
    {
            
    }

    public void SetDockedDrone(DroneController drone)
    {
        _assignedDrone = drone;
    }

    private void PlayAnimation(int value, string animationName, Action callBack)
    {
        if (_isPlaying) return;

        StartCoroutine(PlayAndWaitForAnim(value, animationName, () =>
            {
                _isPlaying = false;
                callBack?.Invoke();
                CloseDoors();
                _animator.SetInteger(_stateHash, 0);
            }
        ));
    }

    public IEnumerator PlayAndWaitForAnim(int valueToSet, string stateName, Action onCompleted)
    {
        _isPlaying = true;
        _animator.SetInteger(_stateHash, valueToSet);

        //Wait until we enter the current state
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            //QuickLogger.Debug("Wait until we enter the current state.");
            yield return null;
        }

        //Now, Wait until the current state is done playing
        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.99f)
        {
            //QuickLogger.Debug("Now, Wait until the current state is done playing");
            yield return null;
        }

        //Done playing. Do something below!
        onCompleted?.Invoke();

        yield break;
    }

}