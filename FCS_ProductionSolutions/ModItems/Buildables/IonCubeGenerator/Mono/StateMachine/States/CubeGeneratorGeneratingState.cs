using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.States;

internal class CubeGeneratorGeneratingState : CubeGeneratorBaseState
{
    [SerializeField] private float generatingStateGoal = 1500f;
    private float generatingProcess = -1f;
    public override void EnterState(CubeGeneratorStateManager manager)
    {
        Debug.Log("Cube Gen State Generating Entered");
    }

    public override void UpdateState(CubeGeneratorStateManager manager)
    {
        if (manager.CubeGeneratorController.IsNotAllowedToGenerate() || !manager.CubeGeneratorController.HasPowerToConsume())
        {
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.Idle));
            return;
        }

        if(generatingProcess < 0)
        {
            generatingProcess = 0;
        }

        if (manager.CubeGeneratorController.GetRequiresEnergy())
        {
            Debug.Log("Taking Energy From Base");
            //ConnectedRelay.ConsumeEnergy(energyToConsume, out float amountConsumed);
        }

        if(generatingProcess < generatingStateGoal)
        {
            // Is currently generating cube
            generatingProcess = Mathf.Min(generatingStateGoal, generatingProcess + manager.CubeGeneratorController.GetEnergyToConsume());
        }
        else
        {
            generatingProcess = -1f;
            manager.SwitchState(manager.GetState(CubeGeneratorStateManager.CubeGeneratorStates.CoolDown));
        }
    }

    public override float GetProgressNormalized()
    {
        return Mathf.Max(0f, generatingProcess / generatingStateGoal);
    }

    internal override void SetProgress(float progess)
    {
        generatingProcess = progess;
    }
}
