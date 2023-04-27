using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Environment;
using Runtime.GameControllers;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Character.AI
{
    [RequireComponent(typeof(CharacterBase))]
    public class EnemyAI: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private float hideThreshold;

        [SerializeField] private float minPlayerHideDist = 1;

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

        public float enemyAttackRange => characterBase.characterStatsBase.weaponData.weaponAttackRange;


        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeCharacterTurn += OnChangeCharacterTurn;
        }

        private void OnDisable()
        {
            TurnController.OnChangeCharacterTurn -= OnChangeCharacterTurn;
        }

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

            if (!characterBase.isInBattle)
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
                //ToDo: Add attack range - probably needs weapon and range for weapon
                if (PlayerInAttackRange())
                {
                    var allTargets = GetAllTargets(true);
                    m_targetCharacter = GetClosestTarget(allTargets);
                    
                    //If they aren't: move to the closest possible position [either behind cover or straight to the player]
                    if (m_targetCharacter != null)
                    {
                        if (InLineOfSight(m_targetCharacter.transform.position))
                        {
                            characterBase.UseCharacterWeapon();
                            yield return new WaitForSeconds(0.5f);
                            characterBase.characterWeaponManager.SelectWeaponTarget(m_targetCharacter.transform);
                            Debug.Log("Should Use Enemy Weapon");

                            yield return new WaitUntil(() => !characterBase.characterWeaponManager.isUsingWeapon);
                            Debug.Log("Done Using Enemy Weapon");
                        }
                        else
                        {
                            characterBase.SetCharacterWalkAction();
                            var coverPos = GetCoverPosition();
                            characterBase.characterMovement.MoveCharacter(coverPos);
                            Debug.Log("Player is in range, but not in light of sight");
                            yield return new WaitUntil(() => !characterBase.characterMovement.isMoving);
                        }

                    }
                    else
                    {
                        characterBase.UseActionPoint();
                        Debug.Log("No Targets For Enemy ATTACK");
                        continue;
                    }
                    
                }
                else
                {
                    var allTargets = GetAllTargets(true);

                    if (allTargets.Count == 0)
                    {
                        characterBase.UseActionPoint();
                        Debug.Log("No Targets For Enemy MOVEMENT");
                        continue;
                    }
                    
                    //Move Enemy Closer
                    if (characterBase.characterStatsBase.enemyPrioritizeCover)
                    {
                        m_targetCharacter = GetClosestTarget(allTargets);
                        characterBase.SetCharacterWalkAction();
                        var coverPos = GetCoverPosition();
                        characterBase.characterMovement.MoveCharacter(coverPos);
                        Debug.Log("Should be moving to closest cover");
                        yield return new WaitUntil(() => !characterBase.characterMovement.isMoving);
                    }
                    else
                    {
                        m_targetCharacter = GetClosestTarget(allTargets);
                        characterBase.SetCharacterWalkAction();
                        Vector3 closestValidPoint = GetClosestValidPos();
                        characterBase.characterMovement.MoveCharacter(closestValidPoint);
                        Debug.Log("Should be moving to closest point to player");
                        yield return new WaitUntil(() => !characterBase.characterMovement.isMoving);
                    }
                    
                }
                
                
                //Look for all cover inside movement range. [if enemy hides behind cover]
                //If none: check in double size, then use two movement slots
                
                
                //This is temporary to check turn order

                if (characterBase.characterActionPoints == 0)
                {
                    characterBase.EndTurn();
                    yield break;
                }

                yield return new WaitForSeconds(1f);

                yield return null;
            }
            
            characterBase.EndTurn();
        }

        //Checks if there is a target in attack range
        private List<CharacterBase> GetAllTargets(bool isPlayerTeam)
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

        private CharacterBase GetClosestTarget(List<CharacterBase> _possibleTargets)
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

        private CharacterBase GetHealthiestTarget(List<CharacterBase> _possibleTargets)
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

        private CharacterBase GetWeakestTarget(List<CharacterBase> _possibleTargets)
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

        private bool PlayerInAttackRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, enemyAttackRange, characterCheckMask);

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

        //Return cover position if enemy is type that hides behind cover
        private Vector3 GetCoverPosition()
        {
            //default pos
            var newMovePos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(transform.position, enemyMovementRange, obstacleCheckMask);
            var targetPos = m_targetCharacter.transform.position;
            int colAmount = colliders.Length;
            int colReduction = 0;
            for (int i = 0; i < colAmount; i++)
            {
                if (Vector3.Distance(colliders[i].transform.position, targetPos) < minPlayerHideDist)
                {
                    colliders[i] = null;
                    colReduction++;
                }
            }

            colAmount -= colReduction;
            
            Array.Sort(colliders, ColliderArraySortComparer);
            
            //Check through all obstacles normals that face away from Target
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colAmount; i++)
                {
                    if (NavMesh.SamplePosition(colliders[i].transform.position, out NavMeshHit hit, 100, NavMesh.AllAreas))
                    {
                        if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                        {
                            Debug.Log("No edge found");
                        }

                        if (Vector3.Dot(hit.normal, (targetPos - hit.position).normalized) < hideThreshold)
                        {
                            return hit.position;
                        }
                        else
                        {
                            if (NavMesh.SamplePosition(colliders[i].transform.position - (targetPos - hit.position).normalized * 2, out NavMeshHit secondAttemptHit, 100, NavMesh.AllAreas))
                            {
                                if (!NavMesh.FindClosestEdge(secondAttemptHit.position, out secondAttemptHit, NavMesh.AllAreas))
                                {
                                    Debug.Log("No edge found");
                                }

                                if (Vector3.Dot(secondAttemptHit.normal, (targetPos - secondAttemptHit.position).normalized) < hideThreshold)
                                {
                                    return secondAttemptHit.position;
                                }
                            }
                        }
                    }
                }
            }

            return newMovePos;
        }

        private Vector3 GetClosestValidPos()
        {
            var newMovePos = transform.position;
            var targetPos = m_targetCharacter.transform.position;
            var dirToTarget = targetPos - newMovePos;
            var farthestMovablePoint = dirToTarget.magnitude > enemyMovementRange ? 
                newMovePos + dirToTarget.normalized * enemyMovementRange:
                newMovePos + dirToTarget.normalized * enemyAttackRange;
            if (NavMesh.SamplePosition(farthestMovablePoint, out NavMeshHit hit, 100, NavMesh.AllAreas))
            {
                if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                {
                    Debug.Log("No edge found");
                }

                return hit.position;
            }

            return newMovePos;
        }
        
        private bool InLineOfSight(Vector3 _checkPos)
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

        private int ColliderArraySortComparer(Collider A, Collider B)
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