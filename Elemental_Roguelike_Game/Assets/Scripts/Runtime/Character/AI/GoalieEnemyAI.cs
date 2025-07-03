using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class GoalieEnemyAI: EnemyAIBase
    {

        #region Accessors

        private LayerMask goalLayer => LayerMask.NameToLayer("GoalArea");

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
                var posAwayFromGoal = (transform.position - enemyTeamGoal.position).normalized * characterBase.characterBallManager.shotStrength;
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

            yield return new WaitUntil(() => characterBase.isDoingAction == false);

            yield return new WaitForSeconds(m_standardWaitTime);

            m_isPerformingAction = false;
        }

        #endregion

        
        
        
        
    }
}