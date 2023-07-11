using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.Environment;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Character.AI
{
    [RequireComponent(typeof(CharacterBase))]
    public abstract class EnemyAIBase: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private LayerMask characterCheckMask;

        [SerializeField] private LayerMask obstacleCheckMask;

        #endregion
        
        #region Private Fields

        private CharacterBase m_characterBase;

        private CharacterBase m_targetCharacter;

        #endregion

        #region Accessors
        
        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase,
            () =>
            {
                var cv = GetComponent<CharacterBase>();
                return cv;
            });

        public bool isMeepleEnemy => characterBase is EnemyCharacterMeeple;

        public float enemyMovementRange => characterBase.characterMovement.battleMoveDistance;
        
        protected Transform playerTeamGoal => TurnController.Instance.GetPlayerManager().goalPosition;

        protected BallBehavior ballReference => TurnController.Instance.ball;
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
        }

        #endregion

        #region Abstract Methods

        public abstract IEnumerator C_PerformEnemyAction();

        #endregion

        #region Class Implementation

        private void OnChangeCharacterTurn(CharacterBase _characterBase)
        {
            if (_characterBase != characterBase)
            {
                return;
            }

            if (!characterBase.isAlive)
            {
                return;
            }

            Debug.Log($"{this} start turn", this);

            StartCoroutine(C_Turn());
        }

        private IEnumerator C_Turn()
        {
            
            characterBase.ResetCharacterActions();

            yield return new WaitForSeconds(0.5f);

            if (!characterBase.isAlive)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            
            while (characterBase.characterActionPoints > 0)
            {

                if (!TurnController.Instance.ball.isControlled)
                {
                    if (IsBallInMovementRange())
                    {
                        yield return C_GoForBall();
                    }
                    else
                    {
                        //ToDo: Change with Different Action
                        yield return C_GoForBall();
                    }
                }
                else
                {
                    //This character has the ball
                    if (!characterBase.heldBall.IsNull())
                    {
                        if (IsInShootRange())
                        {
                            yield return C_ShootBall();
                        }
                        else
                        {
                            yield return C_PositionToScore();
                        }
                    }
                    else
                    {
                        yield return C_PerformEnemyAction();
                    }
                }

                yield return null;
            }
            
            characterBase.EndTurn();
        }

        protected IEnumerator C_GoForBall()
        {
            
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            var ballPosition = ballReference.transform.position;
            var direction = ballPosition - transform.position;
            var adjustedPos = Vector3.zero;
            
            if (direction.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (direction.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = ballPosition;
            }
            
            characterBase.CheckAllAction(adjustedPos, false);

            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

        }

        protected IEnumerator C_GoForBallCarrier()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            var ballCarrierPosition = ballReference.currentOwner.transform.position;
            var direction = ballCarrierPosition - transform.position;
            var adjustedPos = Vector3.zero;
            
            if (direction.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (direction.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = ballCarrierPosition;
            }
            
            characterBase.CheckAllAction(adjustedPos, false);
            
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
        }
        

        protected IEnumerator C_ShootBall()
        {

            characterBase.SetCharacterThrowAction();
            characterBase.CheckAllAction(playerTeamGoal.position , false);

            yield return new WaitUntil(() => characterBase.isSetupThrowBall == false);

        }

        protected IEnumerator C_PositionToScore()
        {
            
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);

            var direction = playerTeamGoal.position - transform.position;
            var adjustedPos = Vector3.zero;
            
            if (direction.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (direction.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = playerTeamGoal.position;
            }
            
            characterBase.CheckAllAction(adjustedPos, false); 
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
        }
        
        
        //Checks if there is a target in attack range
        protected List<CharacterBase> GetAllTargets(bool isPlayerTeam)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, characterCheckMask);
            List<CharacterBase> _targetTransforms = new List<CharacterBase>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    if (isPlayerTeam)
                    {
                        var playerCharacter = col.GetComponent<PlayableCharacter>();
                        if (playerCharacter)
                        {
                            _targetTransforms.Add(playerCharacter);   
                        }
                    }
                    else
                    {
                        var _character = col.GetComponent<CharacterBase>();
                        if (_character is EnemyCharacterRegular || _character is EnemyCharacterMeeple)
                        {
                            _targetTransforms.Add(_character);   
                        }
                    }
                    
                }
            }

            return _targetTransforms;
        }

        protected CharacterBase GetClosestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }

            var bestTarget = _possibleTargets.FirstOrDefault();

            if (_possibleTargets.Count == 1)
            {
                return bestTarget;
            }
            
            foreach (var target in _possibleTargets)
            {
                var _dirToTargetChar = bestTarget.transform.position - transform.position;
                var _dirToCurrentTarget = target.transform.position - transform.position;
                if (_dirToCurrentTarget.magnitude < _dirToTargetChar.magnitude)
                {
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        protected CharacterBase GetHealthiestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }

            var bestTarget = _possibleTargets.FirstOrDefault();

            if (_possibleTargets.Count == 1)
            {
                return bestTarget;
            }
            
            foreach (var target in _possibleTargets)
            {
                if (target.characterLifeManager.currentOverallHealth > bestTarget.characterLifeManager.currentOverallHealth)
                {
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        protected CharacterBase GetWeakestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }

            var bestTarget = _possibleTargets.FirstOrDefault();

            if (_possibleTargets.Count == 1)
            {
                return bestTarget;
            }
            
            foreach (var target in _possibleTargets)
            {
                if (target.characterLifeManager.currentOverallHealth < bestTarget.characterLifeManager.currentOverallHealth)
                {
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        protected bool IsInShootRange()
        {
            bool inRange = false;
            var directionToGoal = playerTeamGoal.position - transform.position;
            var distanceToGoal = directionToGoal.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            if (enemyMovementThreshold >= distanceToGoal || characterBase.shotStrength >= distanceToGoal)
            {
                inRange = true;
            }

            return inRange;
        }

        //ToDo: use to have enemies decide ability
        protected bool PlayerInAbilityRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0, characterCheckMask);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    var playerCharacter = col.GetComponent<PlayableCharacter>();
                    if (playerCharacter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool IsBallInMovementRange()
        {
            bool inMovementRange = false;
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            if (enemyMovementThreshold >= distanceToBall)
            {
                inMovementRange = true;
            }

            return inMovementRange;

        }

        protected bool IsNearEnemyMember()
        {
            return GetAllTargets(true).Count > 0;
        }

        protected bool InLineOfSight(Vector3 _checkPos)
        {
            var dir = transform.position - _checkPos;
            var dirMagnitude = dir.magnitude;
            var dirNormalized = dir.normalized;
            Debug.DrawRay(_checkPos, dirNormalized, Color.red, 10f);
            if (Physics.Raycast(_checkPos, dirNormalized, out RaycastHit hit, dirMagnitude, obstacleCheckMask))
            {
                var _obstacle = hit.transform.GetComponent<CoverObstacles>();
                if (_obstacle != null && _obstacle.type == ObstacleType.FULL)
                {
                    return false;
                }
            }

            return true;
        }

        protected int ColliderArraySortComparer(Collider A, Collider B)
        {
            if (A == null && B != null)
            {
                return 1;
            }else if (A != null && B == null)
            {
                return -1;
            }else if (A == null && B == null)
            {
                return 0;
            }else
            {
                return Vector3.Distance(transform.position, A.transform.position)
                    .CompareTo(Vector3.Distance(transform.position, B.transform.position));
            }
        }

        #endregion
        
        
    }
}