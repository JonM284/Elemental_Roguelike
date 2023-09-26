using System.Collections;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class StrikerEnemyAI: EnemyAIBase
    {

        #region EnemyAIBase Inherited Methods

        public override IEnumerator C_PerformEnemyAction()
        {
            characterBase.ResetCharacterActions();

            yield return new WaitForSeconds(m_standardWaitTime);

            if (!characterBase.isAlive)
            {
                yield break;
            }

            yield return new WaitForSeconds(m_standardWaitTime);

            if (ballReference.isThrown)
            {
                yield return new WaitUntil(() => !ballReference.isThrown);
            }

            if (JuiceController.Instance.isDoingActionAnimation)
            {
                yield return new WaitUntil(() => !JuiceController.Instance.isDoingActionAnimation);
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
                    if (HasAvailableAbilities())
                    {
                        yield return StartCoroutine(C_ConsiderAbilities());
                    }
                    else
                    {
                        characterBase.EndTurn();
                        yield break;
                    }
                }

                if (HasAvailableAbilities())
                {
                    yield return StartCoroutine(C_ConsiderAbilities());
                }
                
                //If the ball isn't controlled
                if (!TurnController.Instance.ball.isControlled)
                {
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
                                yield return StartCoroutine(C_PositionToScore());
                            }
                            else //No player blocking shot
                            {
                                //Check if there's a passable teammate
                                if (HasPassableTeammate())
                                {
                                    yield return StartCoroutine(C_TryPass());
                                }
                                else //No Passible Teammate? Reposition
                                {
                                    yield return StartCoroutine(C_PositionToScore());
                                }
                            }
                        }
                    }
                    else //If this character doesn't have the ball
                    {
                        //Team in possession ISN'T same side as this character
                        if (TurnController.Instance.ball.controlledCharacterSide != this.characterBase.side)
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
                                if (HasAvailableAbilities())
                                {
                                    Debug.Log("<color=orange>Striker Action: Consider Abilities</color>");

                                    yield return StartCoroutine(C_ConsiderAbilities());
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

                yield return new WaitUntil(() => !m_isPerformingAction);
                
                yield return new WaitForSeconds(m_standardWaitTime);
            }

        }

        private IEnumerator C_StayNearClosestEnemy()
        {
            Debug.Log($"<color=orange>{gameObject.name} is staying close to enemy</color>");
            var allTargetsAround = GetAllTargets(true, characterBase.characterMovement.battleMoveDistance);

            if (allTargetsAround.Count == 0)
            {
                characterBase.UseActionPoint();
                yield break;
            }

            yield return null;
            
            characterBase.UseActionPoint();

        }

        #endregion
        
        
        
    }
}