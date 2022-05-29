using System;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine
{
    internal abstract class CrafterBaseState
    {
        protected readonly GameObject _gameObject;
        protected readonly CrafterStateManager _manager;
        public abstract string Name { get; }

        protected CrafterBaseState()
        {

        }

        protected CrafterBaseState(CrafterStateManager manager)
        {
            this._gameObject = manager.gameObject;
            this._manager = manager;
        }

        public abstract void EnterState();
        public abstract Type UpdateState();
    }
}
