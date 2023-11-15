using System.Collections;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class StrikerEnemyAI: EnemyAIBase
    {

        #region EnemyAIBase Inherited Methods

        public override IEnumerator C_PerformEnemyAction()
        {
            yield return new WaitForSeconds(m_standardWaitTime);

            if (!characterBase.isAlive)
            {
                Debug.Log("<color=orange>Striker NOT ALIVE, canceling</color>");
                yield break;
            }

            yield return new WaitForSeconds(m_standardWaitTime);

            if (ballReference.isMoving)
            {
                Debug.Log("<color=orange>Striker waiting for ball to stop</color>");

                yield return new WaitUntil(() => !ballReference.isMoving);
            }

            if (JuiceController.Instance.isDoingActionAnimation)
            {
                Debug.Log("<color=orange>Striker waiting for animation to stop</color>");

                yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
            }

            if (ReactionQueueController.Instance.isDoingReactions)
            {
                Debug.Log("<color=orange>Striker waiting for reactions to end</color>");

                yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
            }
            
            while (characterBase.characterActionPoints > 0 && characterBase.isAlive)
            {
                if (characterBase.characterActionPoints == 0 || !characterBase.isAlive)
                {
                    Debug.Log("<color=orange>Striker has not action points or is NOT alive</color>");

                    yield break;
                }
                
                m_isPerformingAction = true;
                
                if (characterBase.characterMovement.isRooted)
                {
                    var _bestAbility = GetBestAbility();
                    if (!_bestAbility.IsNull())
                    {
                        Debug.Log("<color=orange>Striker ROOTED and has abilities</color>");

                        yield return StartCoroutine(C_ConsiderAbility(_bestAbility));
                    }
                    else
                    {
                        Debug.Log("<color=orange>Striker Rooted and doesn't have abilities</color>");
                        yield break;
                    }
                }

                var _currentBestAbility = GetBestAbility();
                if (!_currentBestAbility.IsNull())
                {
                    Debug.Log("<color=orange>Striker has abilities</color>");

                    yield return StartCoroutine(C_ConsiderAbility(_currentBestAbility));
                }
                else
                {
                    if (!TurnController.Instance.ball.isControlled){ 
                        //If the ball isn't controlled
                        Debug.Log("<color=orange>Striker Action: Ball not controlled, going for ball</color>");
                        
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
                                Debug.Log("<color=orange>Striker shooting ball</color>");

                                yield return StartCoroutine(C_ShootBall());
                            }
                            else
                            {
                                //Is there a player blocking a shot to the goal?
                                if (!HasPlayerBlockingRoute() && !IsNearBruiserCharacter(enemyMovementRange))
                                {
                                    Debug.Log("<color=orange>Striker has player blocking route</color>");

                                    yield return StartCoroutine(C_PositionToScore());
                                }else if (!HasPlayerBlockingRoute() && IsNearBruiserCharacter(enemyMovementRange))
                                {
                                    if (HasPassableTeammate())
                                    {
                                        Debug.Log("<color=orange>Striker has passable teammates, trying to pass</color>");

                                        yield return StartCoroutine(C_TryPass());
                                    }
                                    else //No Passible Teammate? Reposition
                                    {
                                        Debug.Log("<color=orange>Doesn't have passable teammates, positioning to score</color>");

                                        yield return StartCoroutine(C_PositionToScore());
                                    }
                                }
                                else //Player blocking shot
                                {
                                    //Check if there's a passable teammate
                                    if (HasPassableTeammate())
                                    {
                                        Debug.Log("<color=orange>Striker has passable teammates, trying to pass</color>");

                                        yield return StartCoroutine(C_TryPass());
                                    }
                                    else //No Passible Teammate? Reposition
                                    {
                                        Debug.Log("<color=orange>Doesn't have passable teammates, positioning to score</color>");

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
                                Debug.Log("<color=orange>Striker Action: Ball Controlled By other team</color>");
                                
                                //If the ball is in movement range, attack ball carrier
                                if (IsBallInMovementRange())
                                {
                                    Debug.Log("<color=orange>Striker Action: Ball In Movement Range</color>");
                                    
                                    yield return StartCoroutine(C_GoForBallCarrier());
                                }
                                else //Ball is NOT in movement range
                                {
                                    Debug.Log("<color=orange>Striker Action: Ball Outside Movement Range</color>");

                                    //If you can't get the ball, Check if you have abilities
                                    var _bestAbility = GetBestAbility();
                                    if (!_bestAbility.IsNull())
                                    {
                                        Debug.Log("<color=orange>Striker Action: Consider Abilities</color>");

                                        yield return StartCoroutine(C_ConsiderAbility(_bestAbility));
                                    }
                                    else
                                    {
                                        Debug.Log("<color=orange>Striker Action: Go For Ball Carrier</color>");

                                        yield return StartCoroutine(C_GoForBallCarrier());
                                    }
                                }
                            }//This team Controlling ball
                            else
                            {
                                Debug.Log("<color=orange>Striker Action: Ball Owned By this Team</color>");

                                yield return StartCoroutine(C_PositionToScore());
                            }
                        }
                    }    
                }
                
                yield return new WaitUntil(() => !m_isPerformingAction);
                
                yield return new WaitForSeconds(m_standardWaitTime);
                
                if (ballReference.isMoving)
                {
                    Debug.Log("<color=orange>Striker ai waiting for ball</color>");
                    yield return new WaitUntil(() => !ballReference.isMoving);
                }

                if (JuiceController.Instance.isDoingActionAnimation)
                {
                    Debug.Log("<color=orange>Striker AI: waiting for Animation</color>");
                    yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
                }
            
                if (ReactionQueueController.Instance.isDoingReactions)
                {
                    Debug.Log("<color=orange>Striker AI: Waiting for reaction</color>");
                    yield return new WaitUntil(() => !ReactionQueueController.Instance.isDoingReactions);
                }

                if (characterBase.characterMovement.isKnockedBack)
                {
                    Debug.Log("<color=orange>Striker AI: waiting for knockback to end</color>");
                    yield return new WaitUntil(() => !characterBase.characterMovement.isKnockedBack);
                }
            }

        }

        #endregion
        
        
        
    }
}