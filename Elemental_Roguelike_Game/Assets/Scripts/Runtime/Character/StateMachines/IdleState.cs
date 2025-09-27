using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class IdleState: StateBase
    {
        
        #region Private Fields

        private CharacterClassManager classManagerRef;

        #endregion
        
        #region Accessors

        public CharacterClassManager classManager => CommonUtils.GetRequiredComponent(ref classManagerRef,
            GetComponentInParent<CharacterClassManager>);

        #endregion
        
        public override void InitState(StateManager _manager, ECharacterStates eCharacterStates)
        {
            base.InitState(_manager, eCharacterStates);
            //IDLE only checks for throw influence
        }

        public override void EnterState()
        {
            classManager.ResetInfluence();
        }
        
        public override void AssignArgument(params object[] _arguments)
        {
            
        }

        public override void UpdateState()
        {
            classManager.InfluenceUpdate();
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