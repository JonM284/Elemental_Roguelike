using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Utils;
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
            
            Debug.Log($"{this} start turn", this);

            StartCoroutine(C_Turn());
        }

        private IEnumerator C_Turn()
        {
            
            characterBase.ResetCharacterActions();

            if (!characterBase.isAlive)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            
            while (characterBase.characterActionPoints > 0)
            {
                //Check if players are hittable
                //ToDo: Add attack range - probably needs weapon and range for weapon
                if (TargetInRange())
                {
                    
                    //If they are: attack
                
                    //If they aren't: move to the closest possible position [either behind cover or straight to the player]
                    if (m_targetCharacter != null)
                    {
                        characterBase.SetCharacterWalkAction();
                        var coverPos = GetCoverPosition();
                        characterBase.characterMovement.MoveCharacter(coverPos);

                        yield return new WaitUntil(() => !characterBase.characterMovement.isMoving);
                    }
                    
                }
                else
                {
                    characterBase.UseActionPoint();
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
        private bool TargetInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, characterCheckMask);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    var playerCharacter = col.GetComponent<PlayableCharacter>();
                    if (playerCharacter)
                    {
                        m_targetCharacter = playerCharacter;
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, characterBase.characterMovement.battleMoveDistance, obstacleCheckMask);
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