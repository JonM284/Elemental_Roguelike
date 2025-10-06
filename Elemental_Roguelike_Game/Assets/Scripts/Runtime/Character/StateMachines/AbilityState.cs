using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Character.StateMachines
{
    public class AbilityState: StateBase
    {

        #region Private Fields

        private CharacterAbilityManager m_characterAbilityManager;

        private int currentAbilityIndex;
        
        #endregion
        
        #region Accessors

        public CharacterAbilityManager characterAbilityManager => CommonUtils.GetRequiredComponent(ref m_characterAbilityManager,
            GetComponentInParent<CharacterAbilityManager>);

        #endregion
        
        
        #region StateBase Inherited Methods

        public override void InitState(StateManager _manager, ECharacterStates eCharacterStates)
        {
            base.InitState(_manager, eCharacterStates);

            if (stateManager.IsNull())
            {
                return;
            }
            
            if (stateManager.characterBase.characterStatsBase.abilities.Count == 0)
            {
                return;
            }
            
            characterAbilityManager.InitializeCharacterAbilityList(stateManager.characterBase.characterStatsBase.abilities);
        }

        public override void EnterState(params object[] _arguments)
        {
            characterAbilityManager.CancelAbilityUse();
            
            if (_arguments.Length == 0)
            {
                return;
            }

            if (_arguments[0].IsNull())
            {
                return;
            }
            
            currentAbilityIndex = (int)_arguments[0];
            
            characterAbilityManager.ActivateAssignedAbilityAtIndex(currentAbilityIndex, OnAbilityUsed);
        }

        public override void UpdateState()
        {
            
        }

        public override void MarkHighlight(Vector3 _position)
        {
            characterAbilityManager.MarkAbility(_position);
        }

        public override void SelectTarget(Vector3 _position)
        {
            characterAbilityManager.SelectAbilityTarget(_position);
        }

        public override void ExitState()
        {
            if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.CancelAbilityUse();
            }
        }

        #endregion

        #region Class Implementation

        private void OnAbilityUsed()
        {
            stateManager.characterAnimations.AbilityAnim(characterAbilityManager.GetActiveAbilityIndex(), true);
        }

        #endregion
        
    }
}