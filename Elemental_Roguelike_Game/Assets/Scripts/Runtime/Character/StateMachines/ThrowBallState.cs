using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class ThrowBallState: StateBase
    {

        #region Private Fields

        private CharacterBallManager m_characterBallManager;

        #endregion
        
        #region Accessors

        public CharacterBallManager characterBallManager => CommonUtils.GetRequiredComponent(
            ref m_characterBallManager,
            GetComponentInParent<CharacterBallManager>);
        
        #endregion

        #region StateBase Inherited Methods

        public override void InitState(StateManager _manager, ECharacterStates _stateEnum)
        {
            base.InitState(_manager, _stateEnum);

            
        }

        public override void EnterState(params object[] _arguments)
        {
            characterBallManager.DisplayThrowIndicator(true);
        }

        public override void UpdateState()
        {
            
        }

        public override void MarkHighlight(Vector3 _position)
        {
            characterBallManager.MarkThrowBall(_position);
        }

        public override void SelectTarget(Vector3 _position)
        {
            characterBallManager.ThrowBall(_position, characterBallManager.IsShot(_position));
            stateManager.UseActionPoint();
            stateManager.ChangeState(ECharacterStates.Idle);
        }

        public override void ExitState()
        {
            characterBallManager.DisplayThrowIndicator(false);
        }

        #endregion

        #region Class Implementation

        

        #endregion
    }
}