using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class GoalieEnemyAI: EnemyAIBase
    {

        #region Accessors

        private LayerMask goalLayer => LayerMask.NameToLayer("GoalArea");

        #endregion
        
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
                
                if(characterBase.heldBall.IsNull()){
                    yield return StartCoroutine(C_PositionTowardsBallInGoal());
                }
                else
                {
                    yield return StartCoroutine(C_TryPass());
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
            }
        }

        #endregion

        #region Class Implementation

        private IEnumerator C_PositionTowardsBallInGoal()
        {
            Debug.Log("Moving to ball");
            
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var direction = ballReference.transform.position - characterBase.characterMovement.pivotTransform.position;
            var adjustedPos = Vector3.zero;

            adjustedPos = characterBase.characterMovement.pivotTransform.position + (direction.normalized * enemyMovementRange);

            Debug.Log($"Moving to ball2 {adjustedPos}");
            Debug.DrawLine(transform.position, adjustedPos, Color.magenta, 10f);

            characterBase.CheckAllAction(adjustedPos, false);

            yield return new WaitUntil(() => !characterBase.characterMovement.isUsingMoveAction);

            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => !characterBase.characterMovement.isInReaction);
            }

            m_isPerformingAction = false;
        }

        protected new IEnumerator C_TryPass()
        {
            var allAlliesInRange = GetAllTargets(false, 100f);
            List<CharacterBase> passableAllies = new List<CharacterBase>();

            if (allAlliesInRange.Count == 0)
            {
                var posAwayFromGoal = (transform.position - enemyTeamGoal.position).normalized * characterBase.shotStrength;
                characterBase.SetCharacterThrowAction();
                characterBase.CheckAllAction(posAwayFromGoal , false);
                yield break;
            }

            foreach (var ally in allAlliesInRange)
            {
                var dirToGoal = playerTeamGoal.position - transform.position;
                var dirToAlly = ally.transform.position - transform.position;
                var angle = Vector3.Angle(dirToGoal, dirToAlly);

                RaycastHit[] hits = Physics.RaycastAll(transform.position, dirToAlly, dirToAlly.magnitude, goalLayer, QueryTriggerInteraction.Collide);

                if (hits.Length == 0)
                {
                    continue;
                }
                
                if (angle > 45f)
                {
                    passableAllies.Add(ally);
                }
            }

            if (passableAllies.Count == 0)
            {
                StartCoroutine(C_PositionTowardsBallInGoal());
                yield break;
            }

            CharacterBase bestPossiblePass = null;

            foreach (var availableAlly in passableAllies)
            {
                if (!bestPossiblePass.IsNull())
                {
                    continue;
                }
                
                var dirToAlly = availableAlly.transform.position - transform.position;
                if (!IsPlayerInDirection(dirToAlly))
                {
                    bestPossiblePass = availableAlly;
                }
            }

            if (bestPossiblePass.IsNull())
            {
                StartCoroutine(C_PositionTowardsBallInGoal());
                yield break;
            }
            
            Debug.Log("THROW BALL");
            
            characterBase.SetCharacterThrowAction();
            characterBase.CheckAllAction(bestPossiblePass.transform.position , false);

            yield return new WaitUntil(() => characterBase.isSetupThrowBall == false);

            yield return new WaitForSeconds(m_standardWaitTime);

            m_isPerformingAction = false;
        }

        #endregion

        
        
        
        
    }
}