using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;
using UnityEngine;
using static FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.CubeGeneratorStateManager;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.States;
internal class CubeGeneratorCoolDownState : CubeGeneratorBaseState
{
    [SerializeField] private float coolDownStateGoal = 19f;
    private float coolDownProcess = -1f;

    public override void EnterState(CubeGeneratorStateManager manager)
    {
        Debug.Log("Cube Gen State Cooling Entered");
    }

    public override void UpdateState(CubeGeneratorStateManager manager)
    {
        if (manager.CubeGeneratorController.IsNotAllowedToGenerate() || !manager.CubeGeneratorController.HasPowerToConsume())
        {
            manager.SwitchState(manager.GetState(CubeGeneratorStates.Idle));
            return;
        }

        if (coolDownProcess < 0)
        {
            coolDownProcess = 0;
        }

        if (coolDownProcess < coolDownStateGoal)
        {
            // Is currently generating cube
            coolDownProcess = Mathf.Min(coolDownStateGoal, coolDownProcess + DayNightCycle.main.deltaTime);
        }
        else
        {
            coolDownProcess = -1f;
            manager.CubeGeneratorController.SpawnCube();
            manager.SwitchState(manager.GetState(CubeGeneratorStates.Idle));
        }
    }

    public override float GetProgressNormalized()
    {
        return Mathf.Max(0f, coolDownProcess / coolDownStateGoal);
    }

    internal override void SetProgress(float progess)
    {
        coolDownProcess = progess;
    }

    public override float GetProgress()
    {
        return coolDownProcess;
    }
}
