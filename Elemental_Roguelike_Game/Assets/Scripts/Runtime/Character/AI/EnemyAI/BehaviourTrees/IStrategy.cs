using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI.BehaviourTrees
{
    public interface IStrategy
    {
        public Node.Status Process();

        public void Reset()
        {
            //No Operation
        }
    }

    public class TreeAction : IStrategy
    {
        private readonly Action setAction;

        public TreeAction(Action _newAction)
        {
            setAction = _newAction;
        }

        public Node.Status Process()
        {
            setAction?.Invoke();
            return Node.Status.Success;
        }
    }

    public class AwaitingAction : IStrategy
    {
        
        private readonly Action<Action> setAction;
        private bool hasCompletedAction;

        public AwaitingAction(Action<Action> _newAction)
        {
            setAction = _newAction;
        }
        
        public void OnActionFinished()
        {
            hasCompletedAction = true;
        }

        public Node.Status Process()
        {
            setAction?.Invoke(OnActionFinished);
            return hasCompletedAction ? Node.Status.Success : Node.Status.Running;
        }
        
    }

    public class MovementStrategy: IStrategy
    {

        private readonly CharacterBase m_character;
        private readonly float m_movementRange;
        private readonly Vector3 m_targetPosition;
        private Action m_onFinishMovementCallback;

        private bool m_isPerformingAction = true;
        private bool m_hasSetPath;

        public MovementStrategy(CharacterBase _character, float _moveRange, Vector3 _targetPosition, Action _callback = null)
        {
            
            m_character = _character;
            m_movementRange = _moveRange;
            m_targetPosition = _targetPosition;
            m_isPerformingAction = true;
            m_hasSetPath = false;

            Debug.Log($"Moving: {_character.name} to {_targetPosition}");
            m_onFinishMovementCallback = _callback;
        }
        
        public Node.Status Process()
        {
            if (!m_isPerformingAction)
            {
                Debug.Log("<color=#00FF00>Success MOVEMENT</color>");
                m_onFinishMovementCallback?.Invoke();
                return Node.Status.Success;
            }

            if (!m_hasSetPath)
            {
                m_character.characterMovement.SetCharacterMovable(true, null, m_character.UseActionPoint);
            
                var direction = m_targetPosition - m_character.transform.position;
            
                m_character.CheckAllAction(direction.magnitude > m_movementRange ? m_character.transform.position + (direction.normalized * m_movementRange) : m_targetPosition
                    , false);
                
                m_hasSetPath = m_character.characterMovement.isMoving;
                Debug.Log("<color=orange>has set path</color>");
                return Node.Status.Running;
            }

            if (m_character.characterMovement.isUsingMoveAction || m_character.characterMovement.isMoving ||
                m_character.characterMovement.isInReaction)
            {
                Debug.Log("<color=#00FF00>Moving</color>");
                return Node.Status.Running;
            }
            
            Debug.Log("<color=#00FF00>Finished performing action</color>");
            m_isPerformingAction = false;
            return Node.Status.Running;
        }

        public void Reset()
        {
            m_isPerformingAction = true;
            m_hasSetPath = false;
        }
    }
    
    public class Condition : IStrategy
    {
        private readonly Func<bool> predicate;

        public Condition(Func<bool> _predicate)
        {
            this.predicate = _predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }
    
}