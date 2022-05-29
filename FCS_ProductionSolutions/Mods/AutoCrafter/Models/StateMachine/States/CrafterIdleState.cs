using System;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States
{
    internal class CrafterIdleState : CrafterBaseState
    {
        public override string Name { get; }

        public CrafterIdleState()
        {

        }

        internal CrafterIdleState(CrafterStateManager manager) : base(manager)
        {

        }

        public override void EnterState()
        {

        }

        public override Type UpdateState()
        {
            return typeof(CrafterIdleState);
        }
    }
}
