using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.States;
using System;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine;
internal class CubeGeneratorStateManager : MonoBehaviour
{
    private CubeGeneratorBaseState[] states = new CubeGeneratorBaseState[] 
    {
        new CubeGeneratorIdleState(),
        new CubeGeneratorStartUpState(),
        new CubeGeneratorGeneratingState(),
        new CubeGeneratorCoolDownState()
    };

    public enum CubeGeneratorStates
    {
        Idle = 0,
        StartUp = 1,
        Generating = 2,
        CoolDown = 3,
    }



    private CubeGeneratorBaseState currentState;
    public IonCubeGeneratorController CubeGeneratorController { get; private set; }

    private void Awake()
    {
        CubeGeneratorController = GetComponent<IonCubeGeneratorController>();
    }

    void Start()
    {
        currentState = GetState(CubeGeneratorStates.Idle);
        currentState.EnterState(this);
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        currentState.UpdateState(this);
    }

    public CubeGeneratorBaseState GetState(CubeGeneratorStates state)
    {
        return states[(int)state];
    }

    public void SwitchState(CubeGeneratorBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public void SwitchState(int stateIndex,float progess)
    {
        currentState = states[stateIndex];
        currentState.SetProgress(progess);
        currentState.EnterState(this);
    }

    internal CubeGeneratorBaseState GetCurrentState()
    {
        return currentState;
    }

    internal int GetCurrentStateIndex()
    {
        return Array.IndexOf(states,currentState);
    }

    internal bool IsCrafting()
    {
        return currentState == GetState(CubeGeneratorStates.Generating);
    }

    internal bool IsCoolingDown()
    {
        return currentState == GetState(CubeGeneratorStates.CoolDown);
    }

    internal bool IsStartingUp()
    {
        return currentState == GetState(CubeGeneratorStates.StartUp);
    }

    internal float GetCurrentProgress()
    {
        return currentState.GetProgress();
    }

    internal bool IsIdle()
    {
        return currentState == GetState(CubeGeneratorStates.Idle);
    }
}
