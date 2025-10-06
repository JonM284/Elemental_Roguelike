using System;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    [Serializable]
    public abstract class StateBase: MonoBehaviour
    {
        public bool isCompleted { get; protected set; }
        protected float startTime;
        protected float currentTime => Time.time - startTime;
        protected StateManager stateManager;

        public ECharacterStates stateEnum { get; protected set; }

        /// <summary>
        /// When character and stateMachine are initialized. Get All necessary managers for the state at this time
        /// </summary>
        /// <param name="_manager">Inject Manager</param>
        public virtual void InitState(StateManager _manager, ECharacterStates _stateEnum)
        {
            if (!_manager.IsNull())
            {
                stateManager = _manager;
            }

            stateEnum = _stateEnum;
        }
        
        /// <summary>
        /// Called when state is changed to this state
        /// </summary>
        public abstract void EnterState(params object[] _arguments);
        
        /// <summary>
        /// Update this state every frame. NOTE: currently not necessary
        /// </summary>
        public abstract void UpdateState();
        
        /// <summary>
        /// Marks highlight areas for this state
        /// </summary>
        /// <param name="_position">World position of hover position</param>
        public abstract void MarkHighlight(Vector3 _position);
        
        /// <summary>
        /// Called when User selects a location for current state to execute.
        /// </summary>
        /// <param name="_position">Selected World location</param>
        public abstract void SelectTarget(Vector3 _position);
        
        /// <summary>
        /// Called before state is changed.
        /// </summary>
        public abstract void ExitState();
    }
}