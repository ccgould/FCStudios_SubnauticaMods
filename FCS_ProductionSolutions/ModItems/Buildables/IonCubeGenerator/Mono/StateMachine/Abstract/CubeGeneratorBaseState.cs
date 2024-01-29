namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.Abstract;

internal abstract class CubeGeneratorBaseState
{
    public abstract void EnterState(CubeGeneratorStateManager manager);
    public abstract void UpdateState(CubeGeneratorStateManager manager);
    public virtual float GetProgressNormalized()
    {
        return -1;
    }

    public virtual float GetProgress()
    {
        return -1;
    }
    internal virtual void SetProgress(float progess) { }
}
