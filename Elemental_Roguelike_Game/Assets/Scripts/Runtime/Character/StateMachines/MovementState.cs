using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Character.StateMachines
{
    public class MovementState: StateBase
    {

        #region Private Fields

        private CharacterMovement m_characterMovement;
        private Vector3 m_dirToTarget;

        #endregion
        
        #region Accessors

        public CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement,
            GetComponentInParent<CharacterMovement>);

        #endregion

        #region StateBase Inherited Methods
        
        public override void InitState(StateManager _manager, ECharacterStates eCharacterStates)
        {
            base.InitState(_manager, eCharacterStates);
            
            
            characterMovement.InitializeCharacterMovement(_manager.characterBase.characterStatsBase.baseSpeed, 
                _manager.characterBase.characterStatsBase.agilityScore, 
                _manager.characterBase.characterStatsBase.tackleDamageAmount,
                _manager.characterBase.characterStatsBase.typing, false);
        }

        public override void EnterState()
        {
            characterMovement.SetCharacterMovable(true, OnBeginWalkAction, OnWalkActionEnded);
        }
        
        public override void AssignArgument(params object[] _arguments)
        {
            
        }

        public override void UpdateState()
        {
            
        }
        
        public override void MarkHighlight(Vector3 _position)
        {
            characterMovement.MarkMovementLocation(_position);
        }

        public override void SelectTarget(Vector3 _position)
        {
            if (!characterMovement.isUsingMoveAction)
            {
                return;
            }

            //check if this will be a tackle
            m_dirToTarget = _position - transform.position;

            if (m_dirToTarget.magnitude > characterMovement.currentMoveDistance)
            {
                Debug.Log("Too Far");
                return;
            }

            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, _position, 0.2f, m_dirToTarget,
                m_dirToTarget.magnitude);

            bool _isTackle = false;
            var _adjustedFinalPosition = _position;

            foreach (RaycastHit hit in hits)
            {

                //if they are running into an enemy character, make them stop at that character and perform melee
                if (!hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    continue;
                }

                if (!otherCharacter.isTargetable)
                {
                    continue;
                }

                if (otherCharacter.side == stateManager.characterBase.side ||
                    otherCharacter == stateManager.characterBase)
                {
                    continue;
                }

                _adjustedFinalPosition = otherCharacter.transform.position;
                _isTackle = true;
                break;
            }

            characterMovement.MoveCharacter(_adjustedFinalPosition, _isTackle);
            
            CameraUtils.SetCameraTrackPos(characterMovement.transform, true);
        }

        public override void ExitState()
        {
            if (!characterMovement.isUsingMoveAction)
            {
                return;
            }

            characterMovement.SetCharacterMovable(false);
        }

        #endregion

        #region Class Implementation

        protected void OnBeginWalkAction()
        {
            
        }
        
        protected void OnWalkActionEnded()
        {
            stateManager.UseActionPoint();

            stateManager.ChangeState(ECharacterStates.Idle);
        }

        #endregion
        
    }
}