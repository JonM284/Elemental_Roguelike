using System.Collections;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class PlaymakerEnemyAI: EnemyAIBase
    {
        
        #region EnemyAIBase Inherited Methods
        
        public override IEnumerator C_PerformEnemyAction()
        {
            yield return new WaitForSeconds(m_standardWaitTime);

            if (!characterBase.isAlive)
            {
                yield break;
            }

            yield return new WaitForSeconds(m_standardWaitTime);

            if (ballReference.isMoving)
            {
                yield return new WaitUntil(() => !ballReference.isMoving);
            }

            if (JuiceController.Instance.isDoingActionAnimation)
            {
                yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
            }

            if (ReactionQueueController.Instance.isDoingReactions)
            {
                yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
            }
            
            while (characterBase.characterActionPoints > 0 && characterBase.isAlive)
            {
                if (characterBase.characterActionPoints == 0 || !characterBase.isAlive)
                {
                    yield break;
                }
                
                m_isPerformingAction = true;
                
                if (characterBase.characterMovement.isRooted)
                {
                    var _bestRootedAbility = GetBestAbility();
                    if (!_bestRootedAbility.IsNull())
                    {
                        yield return StartCoroutine(C_ConsiderAbility(_bestRootedAbility));
                    }
                    else
                    {
                        yield break;
                    }
                }

                var _currentBestAbility = GetBestAbility();
                if (!_currentBestAbility.IsNull())
                {
                    yield return StartCoroutine(C_ConsiderAbility(_currentBestAbility));
                }
                else
                {
                     //If the ball isn't controlled
                    if (!TurnController.Instance.ball.isControlled)
                    {
                        Debug.Log("<color=orange>Playmaker Action: Ball not controlled, going for ball</color>");
                        yield return StartCoroutine(C_GoForBall());
                    }
                    else //Ball is controlled
                    {
                        
                        //If this character has the ball
                        if (!characterBase.heldBall.IsNull())
                        {
                            //If can shoot
                            if (IsInShootRange())
                            {
                                yield return StartCoroutine(C_ShootBall());
                            }
                            else
                            {
                                //Is there a player blocking a shot to the goal?
                                if (!HasPlayerBlockingRoute())
                                {
                                    yield return StartCoroutine(C_GoToGoal());
                                }
                                else //No player blocking shot
                                {
                                    //Check if there's a passable teammate
                                    if (HasPassableTeammate())
                                    {
                                        yield return StartCoroutine(C_TryPass());
                                    }
                                    else//No Passible Teammate? Reposition
                                    {
                                        yield return StartCoroutine(C_PositionToScore());
                                    }
                                }
                            }
                        }
                        else //If this character doesn't have the ball
                        {
                            //Team in possession ISN'T same side as this character
                            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID)
                            {
                                Debug.Log("<color=orange>Playmaker Action: Ball Controlled By other team</color>");
                                
                                //If the ball is in movement range, attack ball carrier
                                if (IsBallInMovementRange())
                                {
                                    Debug.Log("<color=orange>Playmaker Action: Ball In Movement Range</color>");
                                    yield return StartCoroutine(C_GoForBallCarrier());
                                }
                                else //Ball is NOT in movement range
                                {
                                    Debug.Log("<color=orange>Playmaker Action: Ball Outside Movement Range</color>");

                                    //If you can't get the ball, Check if you have abilities
                                    var _bestAbility = GetBestAbility();
                                    if (!_bestAbility.IsNull())
                                    {
                                        Debug.Log("<color=orange>Playmaker Action: Consider Abilities</color>");

                                        yield return StartCoroutine(C_ConsiderAbility(_bestAbility));
                                    }
                                    else
                                    {
                                        Debug.Log("<color=orange>Playmaker Action: Go For Ball Carrier</color>");

                                        yield return StartCoroutine(C_GoForBallCarrier());
                                    }
                        
                                }
                            }//This team Controlling ball
                            else
                            {
                                Debug.Log("<color=orange>Playmaker Action: Ball Owned By this Team, position to assist</color>");

                                yield return StartCoroutine(C_PositionToAssist());
                            }
                        }
                    }   
                }

                yield return new WaitUntil(() => !m_isPerformingAction);
                
                yield return new WaitForSeconds(m_standardWaitTime);
                
                if (ballReference.isMoving)
                {
                    yield return new WaitUntil(() => !ballReference.isMoving);
                }

                if (JuiceController.Instance.isDoingActionAnimation)
                {
                    yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
                }
            
                if (ReactionQueueController.Instance.isDoingReactions)
                {
                    yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
                }
                
                if (characterBase.characterMovement.isKnockedBack)
                {
                    yield return new WaitUntil(() => !characterBase.characterMovement.isKnockedBack);
                }
            }
        }

        private IEnumerator C_PositionToAssist()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var randomPosition = ballReference.currentOwner.transform.position + 
                                 (Random.insideUnitSphere.FlattenVector3Y() * (enemyMovementRange));
            
            var dirToPosition = randomPosition - transform.position;
            var finalPos = Vector3.zero;
            
            if (dirToPosition.magnitude > enemyMovementRange)
            {
                finalPos = transform.position + (dirToPosition.normalized * enemyMovementRange);
            }
            else
            {
                finalPos = randomPosition;
            }
            
            characterBase.CheckAllAction(finalPos, false);
            
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