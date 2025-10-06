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

        public override void EnterState(params object[] _arguments)
        {
            if (_arguments.IsNull() || _arguments.Length == 0)
            {
                OnKnockbackEnded();
                return;
            }
            
            var knockbackForce = _arguments.Length >= 1 && !_arguments[0].IsNull() ? (float)_arguments[0] : 1f;
            var direction = _arguments.Length >= 2 && !_arguments[1].IsNull() ? (Vector3)_arguments[1] : transform.forward;
            characterMovement.ApplyKnockback(knockbackForce, direction,0.5f, OnKnockbackEnded);
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