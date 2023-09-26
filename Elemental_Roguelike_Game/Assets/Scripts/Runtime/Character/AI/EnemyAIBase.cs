using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Environment;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using Runtime.Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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

        protected CharacterBase m_targetCharacter;

        protected float m_standardWaitTime = 0.3f;

        protected bool m_isPerformingAction;

        #endregion

        #region Protected Fields

        protected bool m_isPerformingAbility;

        #endregion

        #region Accessors
        
        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase,
            () =>
            {
                var cv = GetComponent<CharacterBase>();
                return cv;
            });

        public bool isMeepleEnemy => characterBase is EnemyCharacterMeeple;

        public float enemyMovementRange => characterBase.characterMovement.battleMoveDistance - 0.03f;
        
        protected Transform playerTeamGoal => TurnController.Instance.GetPlayerManager().goalPosition;

        protected Transform enemyTeamGoal => TurnController.Instance.GetTeamManager(characterBase.side).goalPosition;

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
            yield return StartCoroutine(C_PerformEnemyAction());
            
            if (!characterBase.isAlive)
            {
                //Break because character base will take care of death action
                yield break;
            }

            if (!characterBase.finishedTurn)
            {
                characterBase.EndTurn();
            }
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

            yield return new WaitUntil(() => !characterBase.characterMovement.isUsingMoveAction);

            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => !characterBase.characterMovement.isInReaction);
            }

            m_isPerformingAction = false;

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
            
            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;

        }
        

        protected IEnumerator C_ShootBall()
        {

            characterBase.SetCharacterThrowAction();
            characterBase.CheckAllAction(playerTeamGoal.position , false);

            yield return new WaitUntil(() => !ballReference.isThrown);

            yield return new WaitForSeconds(m_standardWaitTime);
            
            m_isPerformingAction = false;

        }

        protected IEnumerator C_TryPass()
        {
            var allAlliesInRange = GetAllTargets(false, characterBase.shotStrength);
            List<CharacterBase> passableAllies = new List<CharacterBase>();

            foreach (var ally in allAlliesInRange)
            {
                var dir = playerTeamGoal.position - transform.position;
                var dirToAlly = ally.transform.position - transform.position;
                var angle = Vector3.Angle(dir, dirToAlly);
                if (angle < 90f)
                {
                    passableAllies.Add(ally);
                }
            }

            if (passableAllies.Count == 0)
            {
                StartCoroutine(C_PositionToScore());
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
                StartCoroutine(C_PositionToScore());
                yield break;
            }
            
            Debug.Log("THROW BALL");
            
            characterBase.SetCharacterThrowAction();
            characterBase.CheckAllAction(bestPossiblePass.transform.position , false);

            yield return new WaitUntil(() => characterBase.isSetupThrowBall == false);

            yield return new WaitForSeconds(m_standardWaitTime);
            
            m_isPerformingAction = false;

        }

        protected bool IsPlayerInDirection(Vector3 direction)
        {
            var dirMagnitude = direction.magnitude;
            var dirNormalized = direction.normalized;
            if (Physics.Raycast(transform.position, dirNormalized, out RaycastHit hit, dirMagnitude, characterCheckMask))
            {
                hit.transform.TryGetComponent(out PlayableCharacter player);
                if (!player.IsNull())
                {
                    return true;
                }
            }

            return false;
        }

        protected IEnumerator C_PositionToScore()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            var randomPosition = Random.insideUnitCircle * characterBase.shotStrength;
            var adjustedRandomPos = new Vector2(Mathf.Abs(randomPosition.x), randomPosition.y);
            var randomPointInShotRange = new Vector3(playerTeamGoal.position.x + adjustedRandomPos.x, playerTeamGoal.position.y, playerTeamGoal.position.z + adjustedRandomPos.y);
            var closestPossiblePoint = randomPointInShotRange;
            var dirToRandomPoint = randomPointInShotRange - transform.position;
            
            Debug.Log(dirToRandomPoint.magnitude);
            Debug.Log(dirToRandomPoint.sqrMagnitude);
            
            if (dirToRandomPoint.magnitude >= enemyMovementRange)
            {
                closestPossiblePoint = transform.position + (dirToRandomPoint.normalized * enemyMovementRange);
            }

            characterBase.CheckAllAction(closestPossiblePoint, false); 
            
            Debug.DrawLine(transform.position, randomPointInShotRange, Color.magenta, 10f);
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        protected bool HasPlayerBlockingRoute()
        {
            var dirToGoal = playerTeamGoal.position - transform.position;
            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, playerTeamGoal.position, 0.2f, dirToGoal, dirToGoal.magnitude);
            
            foreach (RaycastHit hit in hits)
            {
                //if they are running into an enemy character, make them stop at that character and perform melee
                if (hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    if (otherCharacter.side != characterBase.side && otherCharacter != this)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool HasPassableTeammate()
        {
            var allAlliesInRange = GetAllTargets(false, characterBase.shotStrength);
            return allAlliesInRange.Count > 0;
        }
        
        
        //Checks if there is a target in attack range
        protected List<CharacterBase> GetAllTargets(bool isPlayerTeam, float _checkRange)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _checkRange, characterCheckMask);
            List<CharacterBase> _targetTransforms = new List<CharacterBase>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    if (isPlayerTeam)
                    {
                        col.TryGetComponent(out PlayableCharacter playerCharacter);
                        if (playerCharacter)
                        {
                            _targetTransforms.Add(playerCharacter);   
                        }
                    }
                    else
                    {
                        col.TryGetComponent(out CharacterBase _character);
                        if (_character is EnemyCharacterRegular || _character is EnemyCharacterMeeple)
                        {
                            if (_character != this.characterBase)
                            {
                                _targetTransforms.Add(_character);
                            }
                        }
                    }
                    
                }
            }

            return _targetTransforms;
        }

        protected bool HasAvailableAbilities()
        {
            var abilities = characterBase.characterAbilityManager.GetAssignedAbilities();

            CharacterAbilityManager.AssignedAbilities usableAbility = null;
            
            foreach (var ability in abilities)
            {
                if (ability.canUse)
                {
                    return true;
                }
            }

            return false;
        }

        protected IEnumerator C_ConsiderAbilities()
        {
            Debug.Log($"<color=orange>{gameObject.name} is considering abilities</color>");

            var abilities = characterBase.characterAbilityManager.GetAssignedAbilities();

            CharacterAbilityManager.AssignedAbilities usableAbility = null;
            
            
            foreach (var ability in abilities)
            {
                if (!usableAbility.IsNull())
                {
                    continue;
                }
                
                if (ability.canUse)
                {
                    usableAbility = ability;
                }

            }

            if (usableAbility.IsNull())
            {
                //Do Something Else
                Debug.Log("Abilities aren't USEABLE, going for ball");
                if (IsBallInMovementRange())
                {
                    yield return StartCoroutine(C_GoForBall());
                }
                else
                {
                    characterBase.UseActionPoint();    
                }
                yield break;
            }

            var _abilityIndex = abilities.IndexOf(usableAbility);
            Debug.Log($"<color=green>Ability Index:{_abilityIndex}</color>");
            
            //Check what type of ability it is, do thing accordingly
            switch (usableAbility.ability.abilityType)
            {
                case AbilityType.ProjectileDamage: case AbilityType.ProjectileKnockback : case AbilityType.ProjectileStatus: case AbilityType.ApplyStatusEnemy:
                    //Check to see if enemy is in range, if they are: USE ABILITY
                    if (IsNearPlayerMember(usableAbility.ability.range))
                    {
                        m_isPerformingAbility = true;
                        var availableTargets = GetAllTargets(true, usableAbility.ability.range);
                        characterBase.UseCharacterAbility(_abilityIndex);
                        if (usableAbility.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                        {
                            availableTargets.FirstOrDefault().OnSelect();
                        }
                        else
                        {
                            characterBase.CheckAllAction(availableTargets.FirstOrDefault().transform.position, false);
                        }
                        Debug.Log($"<color=green>Ability Index 2:{_abilityIndex}</color>");
                    }
                    break;
                case AbilityType.ZoneHeal: 
                    //Look for allies to help, try to fire as close as possible
                    if (IsNearTeammates(usableAbility.ability.range))
                    {
                        var availableTargets = GetAllTargets(false, usableAbility.ability.range);
                        if (!availableTargets.TrueForAll(cb => cb.characterLifeManager.currentHealthPoints == cb.characterLifeManager.maxHealthPoints))
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.characterLifeManager.currentHealthPoints < cb.characterLifeManager.maxHealthPoints);
                            if (usableAbility.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                            {
                                _actualTarget.OnSelect();
                            }
                            else
                            {
                                characterBase.CheckAllAction(_actualTarget.transform.position, false);
                            }
                        }
                    }
                    break;
                case AbilityType.ApplyStatusTarget:
                    if (IsNearTeammates(usableAbility.ability.range))
                    {
                        var availableTargets = GetAllTargets(false, usableAbility.ability.range);
                        if (!availableTargets.TrueForAll(cb => cb.appliedStatus.IsNull()))
                        {
                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.appliedStatus.status.statusType == StatusType.Negative);
                            if (!_actualTarget.IsNull())
                            {
                                m_isPerformingAbility = true;
                                characterBase.UseCharacterAbility(_abilityIndex);
                                if (usableAbility.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                                {
                                    _actualTarget.OnSelect();
                                }
                                else
                                {
                                    characterBase.CheckAllAction(_actualTarget.transform.position, false);
                                }
                            }
                        }
                    }
                    break;
                case AbilityType.ApplyStatusSelf: case AbilityType.AgilityStatUpgrade: case AbilityType.ThrowStatUpgrade: case AbilityType.DamageStatUpgrade: case AbilityType.MovementUpgrade:
                    //Just apply to self?
                    //Maybe depends?
                    Debug.Log($"<color=orange>Applying Status or Upgrade Self</color>");
                    if (characterBase.appliedStatus.IsNull())
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        Debug.Log($"<color=green>Ability Index 2:{_abilityIndex}</color>");
                    }
                    else
                    {
                        if (characterBase.appliedStatus.status.statusType == StatusType.Negative)
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            Debug.Log($"<color=green>Ability Index 3:{_abilityIndex}</color>");
                        }
                    }
                    break;
                case AbilityType.Teleport:
                    if (IsNearPlayerMember(characterBase.characterMovement.battleMoveDistance))
                    {
                        var nearByPlayers = GetAllTargets(true, characterBase.characterMovement.battleMoveDistance);
                        foreach (var player in nearByPlayers)
                        {
                            if (player.characterClassManager.assignedClass.classType == CharacterClass.BRUISER)
                            {
                                //Use Teleport otherwise unnecessary
                                m_isPerformingAbility = true;
                            }
                        }
                    }
                    break;
                case AbilityType.Movement:
                    //Not sure yet
                    break;
                case AbilityType.ZoneDamage: case AbilityType.DamageCreation: case AbilityType.TrapCreation:
                    //Do Damage
                    if (IsNearPlayerMember(usableAbility.ability.range))
                    {
                        m_isPerformingAbility = true;
                        var availableTargets = GetAllTargets(true, usableAbility.ability.range);
                        characterBase.UseCharacterAbility(_abilityIndex);
                        characterBase.CheckAllAction(availableTargets.FirstOrDefault().transform.position, false);
                    }
                    break;
                case AbilityType.ZoneSelf:
                    if (IsNearPlayerMember(usableAbility.ability.range))
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        characterBase.CheckAllAction(this.transform.position, false);
                    }
                    break;
                case AbilityType.WallCreation:
                    //First check type, 
                    //Bruiser => Deal Damage to Enemy
                    //Defender => Protect Goal
                        
                    break;
            }

            yield return null;

            if (m_isPerformingAbility)
            {
                Debug.Log("Is waiting for ability");
                yield return new WaitUntil(() => !characterBase.characterAbilityManager.isUsingAbilityAction);
            }
            else
            {
                Debug.Log("Can't use FOUND ability: going for ball 2");
                if (IsBallInMovementRange())
                {
                    yield return StartCoroutine(C_GoForBall());
                }
                else
                {
                    characterBase.UseActionPoint();    
                }
                yield break;
            }
           
            Debug.Log("<color=red>Complete Ability Consideration</color>");

            m_isPerformingAbility = false;
            
            yield return new WaitForSeconds(m_standardWaitTime);

            m_isPerformingAction = false;

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

        protected bool IsNearPlayerMember(float _range)
        {
            return GetAllTargets(true, _range).Count > 0;
        }

        protected bool IsNearTeammates(float _range)
        {
            return GetAllTargets(false, _range).Count > 0;
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