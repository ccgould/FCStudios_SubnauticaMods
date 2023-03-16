using System;
using UnityEngine;

namespace FCS_AlterraHub.Core.Systems.DroneSystem.StatesMachine;

internal abstract class BaseState
{
    public abstract string Name { get; }

    protected BaseState()
    {

    }

    protected BaseState(GameObject gameObject)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
    }

    protected GameObject gameObject;
    protected Transform transform;
    public abstract Type Tick();
}
