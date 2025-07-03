using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class IdleState: StateBase
    {
        public override void InitState(StateManager _manager, ECharacterStates eCharacterStates)
        {
            base.InitState(_manager, eCharacterStates);
            //IDLE does nothing
        }

        public override void EnterState()
        {
            
        }
        
        public override void AssignArgument(params object[] _arguments)
        {
            
        }

        public override void UpdateState()
        {
            
        }

        public override void MarkHighlight(Vector3 _position)
        {
            
        }

        public override void SelectTarget(Vector3 _position)
        {
            
        }

        public override void ExitState()
        {
            
        }
    }
}