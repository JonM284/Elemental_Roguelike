using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class StateManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField]
        protected List<StateListItem> m_states = new List<StateListItem>();

        #endregion

        #region Protected Fields

        protected StateListItem m_foundState;
        protected bool m_isRunning;
        protected CharacterAnimations m_characterAnimations;
        
        #endregion

        #region Accessors

        public CharacterBase characterBase { get; private set; }

        public StateListItem currentState { get; private set; }

        public CharacterAnimations characterAnimations => CommonUtils.GetRequiredComponent(ref m_characterAnimations,
            GetComponentInChildren<CharacterAnimations>);
        
        #endregion
        
        #region Unity Events

        void Update()
        {
            if (!m_isRunning)
            {
                return;
            }
            
            if (currentState.IsNull())
            {
                return;
            }
            
            currentState.stateBehavior.UpdateState();
        }
        
        #endregion

        #region Class Implementation

        public async UniTask InitStateMachine(ECharacterStates _startingState, CharacterBase _character)
        {
            characterBase = _character;

            m_foundState = m_states.FirstOrDefault(sli => sli.characterState == _startingState);

            if (m_foundState.IsNull())
            {
                return;
            }
            
            foreach (var _state in m_states)
            {
                _state.stateBehavior.InitState(this, _state.characterState);
                await UniTask.WaitForEndOfFrame();
            }
            
            currentState = m_foundState;
            currentState.stateBehavior?.EnterState();

            m_foundState = null;
            m_isRunning = true;
        }

        public void UninitStateMachine()
        {
            m_isRunning = false;
            currentState.stateBehavior.ExitState();
            currentState = null;
        }
        
        /*public void AddState(ECharacterStates _state, StateBase _stateBehavior)
        {
            if (m_states.Contains(_state))
            {
                Debug.LogError($"Already contains state: {_state.ToString()}");
                return;
            }

            if (_stateBehavior.IsNull())
            {
                return;
            }
            
            m_states.Add(_state, _stateBehavior);
        }*/

        public void ChangeState(ECharacterStates _newState)
        {
            m_foundState = m_states.FirstOrDefault(c => c.characterState == _newState);
            
            if (m_foundState.IsNull())
            {
                Debug.LogError($"Doesn't contain definition for state: {_newState.ToString()}");
                return;
            }

            if (currentState.IsNull())
            {
                return;
            }
            
            Debug.Log($"Exiting State: {currentState.characterState.ToString()}");
            currentState.stateBehavior?.ExitState();
            currentState = m_foundState;
            currentState.stateBehavior?.EnterState();
            m_foundState = null;
            Debug.Log($"Entered State: {currentState.characterState.ToString()}");
        }


        /*public StateBase GetState(ECharacterStates _states)
        {
            if (!m_states.ContainsKey(_states))
            {
                Debug.LogError($"Doesn't contain definition for state: {_states.ToString()}");
                return default;
            }

            m_states.TryGetValue(_states, out m_foundState);

            return m_foundState;
        }*/

        public ECharacterStates GetCurrentStateEnum()
        {
            return currentState.characterState;
        }

        public void UseActionPoint()
        {
            if (characterBase.IsNull())
            {
                return;
            }
            
            characterBase.UseActionPoint();
        }

        #endregion
        
        
        
    }
}