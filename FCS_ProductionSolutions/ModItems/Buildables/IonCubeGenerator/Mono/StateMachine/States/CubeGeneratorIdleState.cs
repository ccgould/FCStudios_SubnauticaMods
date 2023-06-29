using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.States;
internal class CubeGeneratorIdleState : CubeGeneratorBaseState
{
    public override void EnterState(CubeGeneratorStateManager manager)
    {
        Debug.Log("Cube Gen State Idle Entered");
    }

    public override void UpdateState(CubeGeneratorStateManager manager)
    {
        if (manager.CubeGeneratorController.IsNotAllowedToGenerate())
            return;

        if (!manager.CubeGeneratorController.HasPowerToConsume())
            return;


        if (manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.Generating).GetProgressNormalized() > 0)
        {
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.Generating));
            return;
        }

        if (manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.CoolDown).GetProgressNormalized() > 0)
        {
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.CoolDown));
            return;
        }


        manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.StartUp));
    }
}
