using System.Collections;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class BruiserEnemyAI: EnemyAIBase
    {
        #region EnemyAIBase Inherited Methods
        
        public override IEnumerator C_PerformEnemyAction()
        {
            yield return new WaitForSeconds(m_standardWaitTime);

            if (!characterBase.isAlive)
            {
                Debug.Log("<color=orange>Bruiser NOT ALIVE, canceling</color>");
                yield break;
            }

            yield return new WaitForSeconds(m_standardWaitTime);

            if (ballReference.isMoving)
            {
                Debug.Log("<color=orange>Bruiser waiting for ball to stop moving</color>");
                yield return new WaitUntil(() => !ballReference.isMoving);
            }

            if (JuiceController.Instance.isDoingActionAnimation)
            {
                Debug.Log("<color=orange>Bruiser waiting for action animation end</color>");
                yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
            }

            if (ReactionQueueController.Instance.isDoingReactions)
            {
                Debug.Log("<color=orange>Bruiser waiting for reactions to end</color>");
                yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
            }
            
            while (characterBase.characterActionPoints > 0 && characterBase.isAlive)
            {
                if (characterBase.characterActionPoints == 0 || !characterBase.isAlive)
                {
                    Debug.Log("<color=orange>Bruiser action points 0 or dead - canceling coroutine</color>");
                    yield break;
                }

                m_isPerformingAction = true;

                if (characterBase.characterMovement.isRooted)
                {
                    var _rootedBestAbility = GetBestAbility();
                    if (!_rootedBestAbility.IsNull())
                    {
                        Debug.Log("<color=orange>Bruiser is rooted, has abilities</color>");
                        yield return StartCoroutine(C_ConsiderAbility(_rootedBestAbility));
                    }
                    else
                    {
                        Debug.Log("<color=orange>Bruiser is rooted, doesn't have abilities</color>");
                        yield break;
                    }
                }

                var _currentBestAbility = GetBestAbility();
                if (!_currentBestAbility.IsNull())
                {
                    Debug.Log("<color=orange>Bruiser Action: checking abilities</color>");

                    yield return StartCoroutine(C_ConsiderAbility(_currentBestAbility));
                }
                else
                {
                    //If the ball isn't controlled
                    if (!TurnController.Instance.ball.isControlled)
                    {
                        if (IsNearPlayerMember(enemyMovementRange))
                        {
                            Debug.Log("<color=orange>Bruiser Action: Attack nearest enemy</color>");

                            yield return StartCoroutine(C_AttackNearestPlayer());
                        }else
                        {
                            Debug.Log("<color=orange>Bruiser Action: Ball not controlled and close</color>");

                            yield return StartCoroutine(C_GoForBall());
                        }
                    }
                    else //Ball is controlled
                    {
                        
                        //If this character has the ball
                        if (!characterBase.heldBall.IsNull())
                        {
                            //If can shoot
                            if (IsInShootRange())
                            {
                                Debug.Log("<color=orange>Bruiser Action: In Shoot Range</color>");

                                yield return StartCoroutine(C_ShootBall());
                            }
                            else
                            {
                                //Check if there's a passable teammate
                                if (HasPassableTeammate())
                                {
                                    Debug.Log("<color=orange>Bruiser Action: Pass teammate</color>");

                                    yield return StartCoroutine(C_TryPass());
                                }
                                else //No Passible Teammate? Reposition
                                {
                                    Debug.Log("<color=orange>Bruiser Action: Has ball position to score</color>");

                                    yield return StartCoroutine(C_PositionToScore());
                                }
                            }
                        }
                        else //If this character doesn't have the ball
                        {
                            //Team in possession ISN'T same side as this character
                            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID)
                            {
                                Debug.Log("<color=orange>Bruiser Action: Ball Controlled By other team</color>");
                                
                                //If the ball is in movement range, attack ball carrier
                                if (IsBallInMovementRange())
                                {
                                    Debug.Log("<color=orange>Bruiser Action: Ball In Movement Range</color>");
                                    yield return StartCoroutine(C_GoForBallCarrier());
                                }
                                else //Ball is NOT in movement range
                                {
                                    Debug.Log("<color=orange>Bruiser Action: Ball Outside Movement Range</color>");

                                    //If you can't get the ball, Check if you have abilities
                                    var _bestAbility = GetBestAbility();
                                    if (!_bestAbility.IsNull())
                                    {
                                        Debug.Log("<color=orange>Bruiser Action: Consider Abilities</color>");

                                        yield return StartCoroutine(C_ConsiderAbility(_bestAbility));
                                    }
                                    else
                                    {
                                        Debug.Log("<color=orange>Bruiser Action: Go For Ball Carrier</color>");

                                        yield return StartCoroutine(C_GoForBallCarrier());
                                    }
                        
                                }
                            }//This team Controlling ball
                            else
                            {
                                Debug.Log("<color=orange>Bruiser Action: Ball Owned By this Team, protect carrier</color>");

                                yield return StartCoroutine(C_ProtectBallCarrier());
                            }
                        }
                    }
                }

                yield return new WaitUntil(() => !m_isPerformingAction);
                
                yield return new WaitForSeconds(m_standardWaitTime);
                
                if (ballReference.isMoving)
                {
                    Debug.Log("<color=orange>Bruiser ai waiting for ball</color>");
                    yield return new WaitUntil(() => !ballReference.isMoving);
                }

                if (JuiceController.Instance.isDoingActionAnimation)
                {
                    Debug.Log("<color=orange>Bruiser AI: waiting for Animation</color>");
                    yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
                }
            
                if (ReactionQueueController.Instance.isDoingReactions)
                {
                    Debug.Log("<color=orange>Bruiser AI: Waiting for reaction</color>");
                    yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
                }
                
                if (characterBase.characterMovement.isKnockedBack)
                {
                    Debug.Log("<color=orange>Bruiser AI: waiting for knockback to end</color>");
                    yield return new WaitUntil(() => !characterBase.characterMovement.isKnockedBack);
                }
            }
        }

        private IEnumerator C_AttackNearestPlayer()
        {
            var allTargets = GetAllTargets(true, enemyMovementRange);

            if (allTargets.IsNull() || allTargets.Count <= 0)
            {
                yield break;
            }

            var weakestTarget = GetWeakestTarget(allTargets);

            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var _targetPosition = weakestTarget.transform.position;
            var direction = _targetPosition - transform.position;
            var adjustedPos = Vector3.zero;
            
            if (direction.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (direction.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = _targetPosition;
            }
            
            characterBase.CheckAllAction(adjustedPos, false);
            
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
            
            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        #endregion
    }
}