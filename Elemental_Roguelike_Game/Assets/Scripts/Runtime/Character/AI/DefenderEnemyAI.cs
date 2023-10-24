using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class DefenderEnemyAI: EnemyAIBase
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
                    var _rootedBestAbility = GetBestAbility();
                    if (!_rootedBestAbility.IsNull())
                    {
                        yield return StartCoroutine(C_ConsiderAbility(_rootedBestAbility));
                    }
                    else
                    {
                        characterBase.EndTurn();
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
                        if (IsBallInMovementRange())
                        {
                            yield return StartCoroutine(C_GoForBall());
                        }
                        else
                        {
                            yield return StartCoroutine(C_RepositionTowardsMidField());
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
                            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID)
                            {
                                //If the ball is in movement range, attack ball carrier
                                if (IsBallInMovementRange())
                                {
                                    yield return StartCoroutine(C_GoForBallCarrier());
                                }
                                else //Ball is NOT in movement range
                                {
                                    //If you can't get the ball, Check if you have abilities
                                    var _bestAbility = GetBestAbility();
                                    if (!_bestAbility.IsNull())
                                    {
                                        yield return StartCoroutine(C_ConsiderAbility(_bestAbility));
                                    }
                                    else if (IsBallCarrierStriker())
                                    {
                                        yield return StartCoroutine(C_RepositionToBlockShot());
                                    }
                                    else
                                    {
                                        yield return StartCoroutine(C_RepositionToBlockPass());
                                    }

                                }
                            }//This team Controlling ball
                            else
                            {
                                yield return StartCoroutine(C_RepositionTowardsMidField());
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


        private IEnumerator C_RepositionTowardsMidField()
        {
            Debug.Log("<color=green>Defender AI: running to mid field</color>");

            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var randomPosition = Random.insideUnitSphere.FlattenVector3Y() * enemyMovementRange;
            var closestPossiblePoint = randomPosition;
            var dirToRandomPoint = randomPosition - transform.position;

            if (dirToRandomPoint.magnitude >= enemyMovementRange)
            {
                closestPossiblePoint = transform.position + (dirToRandomPoint.normalized * enemyMovementRange);
            }

            characterBase.CheckAllAction(closestPossiblePoint, false); 
            
            Debug.DrawLine(transform.position, closestPossiblePoint, Color.magenta, 10f);
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        private IEnumerator C_RepositionToBlockPass()
        {
            Debug.Log("<color=green>Defender AI blocking pass</color>");
            
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var blockPoint = GetInterceptLocation().FlattenVector3Y();
            var direction = blockPoint - transform.position;
            var adjustedPos = Vector3.zero;
            
            if (direction.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (direction.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = blockPoint;
            }
            
            characterBase.CheckAllAction(adjustedPos, false);
            
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
            
            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        private IEnumerator C_RepositionToBlockShot()
        {
            Debug.Log("<color=green>Defender AI: blocking shot</color>");

            ballReference.currentOwner.TryGetComponent(out CharacterBase _ballCarrierCharacter);

            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);

            yield return new WaitForSeconds(m_abilityWaitTime);
            
            var ballCarrierPosition = _ballCarrierCharacter.transform.position;
            var direction = ballCarrierPosition - TurnController.Instance.GetTeamManager(characterBase.side).transform.position;
            var blockPosition = direction / 2;
            var directionToBlockPoint = blockPosition - transform.position;
            var adjustedPos = blockPosition;
            
            if (directionToBlockPoint.magnitude > enemyMovementRange)
            {
                adjustedPos = transform.position + (directionToBlockPoint.normalized * enemyMovementRange);
            }
            else
            {
                adjustedPos = blockPosition;
            }
            
            characterBase.CheckAllAction(adjustedPos, false);
            
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
            
            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        private Vector3 GetInterceptLocation()
        {
            if (ballReference.IsNull())
            {
                return Vector3.zero;
            }

            if (ballReference.currentOwner.IsNull())
            {
                return Vector3.zero;
            }

            List<CharacterBase> passableEnemies = new List<CharacterBase>();

            if (ballReference.currentOwner.TryGetComponent(out CharacterBase _ballCarrier))
            {
                passableEnemies = GetAllTargets(false, _ballCarrier.passStrength);
            }

            if (passableEnemies.Count <= 0)
            {
                return transform.position;
            }

            //Middle point
            return (passableEnemies.FirstOrDefault().transform.position - _ballCarrier.transform.position)/2;

        }

        private bool IsBallCarrierStriker()
        {
            if (ballReference.IsNull())
            {
                return false;
            }

            if (ballReference.currentOwner.IsNull())
            {
                return false;
            }

            if (ballReference.currentOwner.TryGetComponent(out CharacterBase _ballCarrier))
            {
                if (_ballCarrier.characterClassManager.assignedClass.classType == CharacterClass.STRIKER)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        
        #endregion
    }
}