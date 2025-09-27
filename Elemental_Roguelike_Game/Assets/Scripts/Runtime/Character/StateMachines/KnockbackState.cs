using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class KnockbackState: StateBase
    {
        
         #region Private Fields

        private CharacterMovement m_characterMovement;

        #endregion
        
        #region Accessors

        public CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement,
            GetComponentInParent<CharacterMovement>);

        #endregion

        #region StateBase Inherited Methods
        
        public override void InitState(StateManager _manager, ECharacterStates eCharacterStates)
        {
            base.InitState(_manager, eCharacterStates);
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

        #endregion

        #region Class Implementation

        protected void OnBeginKnockback()
        {
            
        }
        
        protected void OnKnockbackEnded()
        {
            stateManager.ChangeState(ECharacterStates.Idle);
        }

        #endregion
        
        
        
    }
}