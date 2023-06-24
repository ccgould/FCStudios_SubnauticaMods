using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.States;
internal class CubeGeneratorStartUpState : CubeGeneratorBaseState
{
    [SerializeField] private float startUpStateGoal = 4f;
    private float startUpProcess = -1f;
    public override void EnterState(CubeGeneratorStateManager manager)
    {
        Debug.Log("Cube Gen State StartUp Entered");
    }

    public override void UpdateState(CubeGeneratorStateManager manager)
    {
        if (manager.CubeGeneratorController.IsNotAllowedToGenerate() || !manager.CubeGeneratorController.HasPowerToConsume())
        {
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.Idle));
            return;
        }

        if (startUpProcess < 0)
        {
            startUpProcess = 0;
        }

        if (startUpProcess < startUpStateGoal)
        {
            // Is currently generating cube
            startUpProcess = Mathf.Min(startUpStateGoal, startUpProcess + DayNightCycle.main.deltaTime);
        }
        else
        {
            startUpProcess = -1f;
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.Generating));
        }
    }

    public override float GetProgressNormalized()
    {
        return Mathf.Max(0f, startUpProcess / startUpStateGoal);
    }

    internal override void SetProgress(float progess)
    {
        startUpProcess = progess;
    }
}
