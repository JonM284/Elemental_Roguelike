using System;

namespace Runtime.Character.StateMachines
{
    [Serializable]
    public class StateListItem
    {
        public ECharacterStates characterState;
        public StateBase stateBehavior;

        public StateListItem(ECharacterStates _stateEnum, StateBase _stateBehavior)
        {
            characterState = _stateEnum;
            stateBehavior = _stateBehavior;
        }
    }
}