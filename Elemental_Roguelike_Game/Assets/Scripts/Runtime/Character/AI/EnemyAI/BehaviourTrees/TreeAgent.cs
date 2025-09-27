using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Status;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI.BehaviourTrees
{
    public class TreeAgent: MonoBehaviour
    {
        
        #region Serialized Fields

        [SerializeField] private LayerMask characterCheckMask;

        [SerializeField] private LayerMask obstacleCheckMask;

        #endregion
        
        #region Private Fields

        private CharacterBase m_characterBase;

        protected CharacterBase m_targetCharacter;

        protected float m_standardWaitTime = 0.3f;

        protected float m_abilityWaitTime = 0.2f;

        protected bool m_isPerformingAction;

        protected BehaviourTree m_tree;

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

        public float enemyMovementRange => characterBase.characterMovement.currentMoveDistance - 0.07f;
        
        protected Transform playerTeamGoal => TurnController.Instance.GetPlayerManager().goalPosition;

        protected Transform enemyTeamGoal => TurnController.Instance.GetTeamManager(characterBase.side).goalPosition;

        protected BallBehavior ballReference => TurnController.Instance.ball;

        protected bool canPerformNextAction =>
            !m_isPerformingAction && !m_isPerformingAbility && characterBase.characterActionPoints > 0;

        protected bool canContinueTurn =>
            (m_isPerformingAction || m_isPerformingAbility) && characterBase.characterActionPoints > 0;
        
        #endregion

        #region Unity Events

        private void Awake()
        {
            SetupBehaviorTrees();
        }
        
        private void OnEnable()
        {
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
        }

        #endregion

        #region Class Implementation

        private void OnChangeCharacterTurn(CharacterBase _characterBase)
        {
            if (characterBase.IsNull())
            {
                return;
            }
            
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
            yield return new WaitForSeconds(m_standardWaitTime);

            while (characterBase.characterActionPoints > 0)
            {
                if (characterBase.characterActionPoints <= 0 || !characterBase.isAlive)
                {
                    break;
                }

                if (m_isPerformingAction)
                {
                    continue;
                }

                if (m_isPerformingAction)
                {
                    continue;
                }

                if (m_tree.Process() == Node.Status.Running)
                {
                    continue;
                }
                
                Debug.Log("Processing tree");
                m_tree.Process();
            }
            
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
        
        protected void SetupBehaviorTrees()
        {
            m_tree = new BehaviourTree("Test Agent");

            PrioritySelector _availableActions = new PrioritySelector("Test Agent");

            //Go grab ball, if ball dropped
            Sequence _getBall = new Sequence("Get dropped ball", 100);
            _getBall.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction)));
            _getBall.AddChild(new Leaf("isBallGetable?", new Condition(() => !TurnController.Instance.ball.isControlled && IsBallInMovementRange())));
            _getBall.AddChild(new Leaf("GetBall", new TreeAction(() => 
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_GoForBall());
            })));

            _availableActions.AddChild(_getBall);

            //-----------------
            //Go for ball carrier, if other team has ball 
            //-> random between ability and tackle if ability is damaging
            RandomSelector _attackForBall = new RandomSelector("Attack For Ball", 50);
            
            //--- First Option
            Sequence _tackle = new Sequence("PerformTackle");
            _tackle.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction)));
            _tackle.AddChild(new Leaf("isBallGetable?", new Condition(() => TurnController.Instance.ball.isControlled 
                                                                            && TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID 
                                                                            && IsBallInMovementRange())));
            _tackle.AddChild(new Leaf("TackleBallCarrier", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_GoForBallCarrier());
            })));
            
            _attackForBall.AddChild(_tackle);

            //--- Second Option
            Sequence _attackCarrierwAbility = new Sequence("UseAbilityForBall");
            _attackCarrierwAbility.AddChild(new Leaf("HasGoodAbility?", new Condition(() => !GetBestAbility().IsNull())));
            _attackCarrierwAbility.AddChild(new Leaf("AttackWithAbility", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_ConsiderAbility(GetBestAbility()));
            })));
            
            _attackForBall.AddChild(_attackCarrierwAbility);

            _availableActions.AddChild(_attackForBall);
            
            //--------------
            //Go for goal, if this team has ball
            PrioritySelector _scoreGoal = new PrioritySelector("Score Goal", 1);

            Sequence _shootBall = new Sequence("ShootBall", 600);
            _shootBall.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall)));
            _shootBall.AddChild(new Leaf("CanShoot?", new Condition(IsInShootRange)));
            _shootBall.AddChild(new Leaf("ShootBall", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_ShootBall());
            })));

            _scoreGoal.AddChild(_shootBall);
            
            Sequence _runBallIn = new Sequence("RunBall", 500);
            _runBallIn.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall)));
            _runBallIn.AddChild(new Leaf("CanRunBallIn?", new Condition(() => !HasPlayerBlockingRoute() && !IsNearBruiserCharacter(enemyMovementRange))));
            _runBallIn.AddChild(new Leaf("RunBallIn", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_GoToGoal());
            })));
            
            _scoreGoal.AddChild(_runBallIn);
            
            Sequence _passBall = new Sequence("PassBall", 100);
            _passBall.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall)));
            _passBall.AddChild(new Leaf("CanPass?", new Condition(() => (HasPlayerBlockingRoute() || IsNearBruiserCharacter(enemyMovementRange)) && HasPassableTeammate())));
            _passBall.AddChild(new Leaf("PassBall", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_TryPass());
            })));

            _scoreGoal.AddChild(_passBall);
            
            Sequence _repositionToScore = new Sequence("Reposition", 50);
            _repositionToScore.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall)));
            _repositionToScore.AddChild(new Leaf("CanReposition?", new Condition(() => HasPlayerBlockingRoute() && !IsNearBruiserCharacter(enemyMovementRange) && !HasPassableTeammate())));
            _repositionToScore.AddChild(new Leaf("RepositionToScore", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_PositionToScore());
            })));
            
            _scoreGoal.AddChild(_repositionToScore);
            
            _availableActions.AddChild(_scoreGoal);

            //--------------
            //If AI has healing or buffing ability, use?
            Sequence _considerHealingOrBuffing = new Sequence("ConsiderAbility", 5);
            _considerHealingOrBuffing.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction)));
            _considerHealingOrBuffing.AddChild(new Leaf("CanHealSomething?", new Condition(HasUsableHealingAbility)));
            _considerHealingOrBuffing.AddChild(new Leaf("HealSomething", new TreeAction(() =>
            {
                m_isPerformingAction = true;
                PerformCoroutine(C_ConsiderAbility(GetBestAbility()));
            })));

            _availableActions.AddChild(_considerHealingOrBuffing);

            
            //--------------
            //If can not go for ball, and Enemy is in range, go for enemy
            Sequence _attackRandomCloseEnemy = new Sequence("RandomAttack", 10);
            _attackRandomCloseEnemy.AddChild(new Leaf("isCanDoAction?", new Condition(() => canPerformNextAction)));
            _attackRandomCloseEnemy.AddChild(new Leaf("HasEnemyNearby?", new Condition(() => HasUsableDamagingAbility() && !GetBestAbility().IsNull() && IsNearPlayerMember(GetBestAbility().abilityUseRange))));
            _attackRandomCloseEnemy.AddChild(new Leaf("AttackRandom", new TreeAction(() =>
            {
                m_isPerformingAbility = true;
                PerformCoroutine(C_ConsiderAbility(GetBestAbility()));
            })));
            
            _availableActions.AddChild(_attackRandomCloseEnemy);

            //--------------
            //Other wise skip turn, this will happen if we have nothing else we can do
            Sequence _skipTurn = new Sequence("Skip_Turn");
            
            _skipTurn.AddChild(new Leaf("Has nothing they can do", new Condition(() => characterBase.characterActionPoints > 0 
                && characterBase.characterMovement.isRooted && GetBestAbility().IsNull())));
            
            _skipTurn.AddChild(new Leaf("SkipTurn", new TreeAction(() => characterBase.EndTurn())));
            
            _availableActions.AddChild(_skipTurn);
            
            //--------------
            //Wait until this character can do something again
            Sequence _idle = new Sequence("Idle");
            _idle.AddChild(new Leaf("isWaitingToContinue", new Condition(() => canContinueTurn)));

            _availableActions.AddChild(_idle);
            
            m_tree.AddChild(_availableActions);

        }

        #endregion

        #region Performable Actions - Functions

        private void PerformCoroutine(IEnumerator _enumerator)
        {
            StartCoroutine(_enumerator);
        }

        #endregion
        
        #region Performable Actions - Coroutines

        protected IEnumerator C_GoForBall()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
         
            yield return new WaitForSeconds(m_abilityWaitTime);

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
            
            yield return new WaitForSeconds(m_abilityWaitTime);

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
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            characterBase.CheckAllAction(playerTeamGoal.position , false);

            yield return new WaitUntil(() => !ballReference.isThrown);

            yield return new WaitForSeconds(m_standardWaitTime);
            
            m_isPerformingAction = false;

        }

        protected IEnumerator C_TryPass()
        {
            var allAlliesInRange = GetAllTargets(false, characterBase.characterBallManager.passStrength);
            List<CharacterBase> passableAllies = new List<CharacterBase>();

            foreach (var ally in allAlliesInRange)
            {
                var dir = playerTeamGoal.position - transform.position;
                var dirToAlly = ally.transform.position - transform.position;
                var angle = Vector3.Angle(dir, dirToAlly);
                if (angle is < 150f and > 25f)
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
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            characterBase.CheckAllAction(bestPossiblePass.transform.position , false);

            yield return new WaitUntil(() => characterBase.isDoingAction == false);

            yield return new WaitForSeconds(m_standardWaitTime);
            
            m_isPerformingAction = false;

        }
        
        protected IEnumerator C_ConsiderAbility(CharacterAbilityManager.AssignedAbilities _ability)
        {
            Debug.Log($"<color=orange>{gameObject.name} is considering abilities</color>");

            var abilities = characterBase.characterAbilityManager.GetAssignedAbilities();

            if (_ability.IsNull())
            {
                //Do Something Else
                Debug.Log("Ability isn't valid, going for ball");
                yield return StartCoroutine(C_GoForBall());
                yield break;
            }

            var _abilityIndex = abilities.IndexOf(_ability);
            Debug.Log($"<color=green>Ability Index:{_abilityIndex}</color>");
            
            //Check what type of ability it is, do thing accordingly
            switch (_ability.ability.abilityType)
            {
                case AbilityType.ProjectileDamage: case AbilityType.ProjectileKnockback : case AbilityType.ProjectileStatus: case AbilityType.ApplyStatusEnemy:
                    //Check to see if enemy is in range, if they are: USE ABILITY
                    if (IsNearPlayerMember(_ability.ability.range - 0.05f))
                    {
                        var availableTargets = GetAllTargets(true, _ability.ability.range - 0.07f);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }

                        CharacterBase bestTarget = null;

                        if (TurnController.Instance.ball.isControlled)
                        {
                            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != characterBase.side.sideGUID)
                            {
                                //other team has ball
                                bestTarget = availableTargets.TrueForAll(cb => !cb.characterBallManager.hasBall) ? availableTargets.FirstOrDefault() : availableTargets.FirstOrDefault(cb => cb.characterBallManager.hasBall);
                            }
                            else
                            {
                                bestTarget = availableTargets.FirstOrDefault();
                            }
                        }
                        else
                        {
                            bestTarget = availableTargets.FirstOrDefault();
                        }

                        if (bestTarget.IsNull())
                        {
                            break;
                        }
                        
                        if (_ability.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            yield return new WaitForSeconds(m_abilityWaitTime);
                            bestTarget.OnSelect();
                        }
                        else
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            yield return new WaitForSeconds(m_abilityWaitTime);
                            characterBase.CheckAllAction(bestTarget.transform.position, false);
                        }
                        
                        Debug.Log($"<color=green>Ability Index 2:{_abilityIndex}</color>");
                    }

                    break;
                
                case AbilityType.ZoneHeal: 
                    //Look for allies to help, try to fire as close as possible
                    if (IsNearTeammates(_ability.ability.range - 0.07f))
                    {
                        var availableTargets = GetAllTargets(false, _ability.ability.range);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        if (!availableTargets.TrueForAll(cb => cb.characterLifeManager.currentHealthPoints == cb.characterLifeManager.maxHealthPoints))
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            
                            yield return new WaitForSeconds(m_abilityWaitTime);

                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.characterLifeManager.currentHealthPoints < cb.characterLifeManager.maxHealthPoints);
                            if (_ability.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
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
                    if (IsNearTeammates(_ability.ability.range - 0.07f))
                    {
                        var availableTargets = GetAllTargets(false, _ability.ability.range);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        if (!availableTargets.TrueForAll(cb => cb.appliedStatus.IsNull()))
                        {
                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.appliedStatus.status.statusType == StatusType.Negative);
                            if (!_actualTarget.IsNull())
                            {
                                m_isPerformingAbility = true;
                                characterBase.UseCharacterAbility(_abilityIndex);
                                
                                yield return new WaitForSeconds(m_abilityWaitTime);

                                
                                if (_ability.ability.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
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
                    if (TurnController.Instance.ball.isControlled && !characterBase.characterBallManager.hasBall)
                    {
                        //If I don't have the ball, ignore use
                        break;
                    }

                    if (IsNearPlayerMember(enemyMovementRange))
                    {
                        var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }

                        foreach (var _target in availableTargets)
                        {
                            if (_target.characterClassManager.assignedClass.classType == CharacterClass.BRUISER)
                            {
                                var dirToBruiser = _target.transform.position - transform.position;
                                if (!(dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius))
                                {
                                    continue;
                                }
                                
                                if (!TurnController.Instance.ball.isControlled)
                                {
                                    var dirToBall = TurnController.Instance.ball.transform.position.FlattenVector3Y() -
                                                    transform.position.FlattenVector3Y();
                                    m_isPerformingAbility = true;
                                    characterBase.UseCharacterAbility(_abilityIndex);
                                    yield return new WaitForSeconds(m_abilityWaitTime);
                                    var targetPos = dirToBall.normalized * _ability.ability.range;
                                    characterBase.CheckAllAction(targetPos, false);
                                }
                                else
                                {
                                    if (characterBase.characterBallManager.hasBall)
                                    {
                                        var dirToGoal = TurnController.Instance.GetPlayerManager().goalPosition.position.FlattenVector3Y() -
                                                        transform.position.FlattenVector3Y();
                                        m_isPerformingAbility = true;
                                        characterBase.UseCharacterAbility(_abilityIndex);
                                        yield return new WaitForSeconds(m_abilityWaitTime);
                                        Vector3 targetPos = Vector3.zero;
                                        if (dirToGoal.magnitude > _ability.ability.range - 0.07f)
                                        {
                                            targetPos = dirToGoal.normalized * _ability.ability.range;
                                        }
                                        else
                                        {
                                            targetPos = TurnController.Instance.GetPlayerManager().goalPosition
                                                .position.FlattenVector3Y();
                                        }
                                            
                                        characterBase.CheckAllAction(targetPos, false);
                                    }
                                        
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case AbilityType.ZoneDamage: case AbilityType.DamageCreation: case AbilityType.TrapCreation: case AbilityType.WallCreation:
                    //Do Damage
                    if (IsNearPlayerMember(_ability.ability.range - 0.07f))
                    {
                        var availableTargets = GetAllTargets(true, _ability.ability.range);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        
                        yield return new WaitForSeconds(m_abilityWaitTime);

                        var targetPos = (availableTargets.FirstOrDefault().transform.position +
                                         Random.insideUnitSphere).FlattenVector3Y();
                        characterBase.CheckAllAction(targetPos, false);
                    }
                    break;
                case AbilityType.ZoneSelf:
                    if (IsNearPlayerMember(_ability.ability.range - 0.07f))
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);

                        yield return new WaitForSeconds(m_abilityWaitTime);

                        characterBase.CheckAllAction(this.transform.position, false);
                    }
                    break;
                case AbilityType.Pull:
                    if (IsBallInRange(_ability.ability.range - 0.07f))
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        
                        yield return new WaitForSeconds(m_abilityWaitTime);

                        characterBase.CheckAllAction(ballReference.transform.position, false);
                    }
                    break;
            }

            yield return null;

            if (m_isPerformingAbility)
            {
                Debug.Log("Is waiting for ability");
                yield return new WaitUntil(() => !characterBase.characterAbilityManager.isUsingAbilityAction || characterBase.characterAbilityManager.hasCanceledAbility);
                
                if (characterBase.characterAbilityManager.hasCanceledAbility)
                {
                    Debug.Log("<color=red>Ability Canceled</color>");
                    yield return StartCoroutine(C_GoForBall());
                    yield break;
                }
            }
            else
            {
                Debug.Log("Can't use FOUND ability: going for ball 2");
                yield return StartCoroutine(C_GoForBall());
                yield break;
            }
           
            Debug.Log("<color=red>Complete Ability Consideration</color>");

            yield return new WaitForSeconds(m_standardWaitTime);

            m_isPerformingAction = false;

        }

        protected IEnumerator C_GoToGoal()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var dirToGoal = playerTeamGoal.transform.position - transform.position;
            var finalPos = Vector3.zero;
            
            if (dirToGoal.magnitude > enemyMovementRange)
            {
                finalPos = transform.position + (dirToGoal.normalized * enemyMovementRange);
            }
            else
            {
                finalPos = playerTeamGoal.transform.position;
            }
            
            characterBase.CheckAllAction(finalPos, false);
            
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
            
            if (characterBase.characterMovement.isInReaction)
            {
                yield return new WaitUntil(() => characterBase.characterMovement.isInReaction == false);
            }
            
            m_isPerformingAction = false;
        }

        protected IEnumerator C_PositionToScore()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var randomPosition = Random.insideUnitCircle * characterBase.characterBallManager.shotStrength;
            var adjustedRandomPos = new Vector2(Mathf.Abs(randomPosition.x), randomPosition.y);
            var randomPointInShotRange = new Vector3(playerTeamGoal.position.x + adjustedRandomPos.x, playerTeamGoal.position.y, playerTeamGoal.position.z + adjustedRandomPos.y);
            var closestPossiblePoint = randomPointInShotRange;
            var dirToRandomPoint = randomPointInShotRange - transform.position;

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

        protected IEnumerator C_ProtectBallCarrier()
        {
            characterBase.characterMovement.SetCharacterMovable(true, null, characterBase.UseActionPoint);
            
            yield return new WaitForSeconds(m_abilityWaitTime);

            var randomPosition = ballReference.currentOwner.transform.position + 
                                 (Random.insideUnitSphere.FlattenVector3Y() * (characterBase.characterClassManager.assignedClass.overwatchRadius/0.5f));
            
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

        #region Conditions
        
        protected bool IsBallInMovementRange()
        {
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            return enemyMovementThreshold >= distanceToBall;
        }
        
        protected bool IsBallInRange(float _range)
        {
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            return _range >= distanceToBall;
        }

        protected bool IsNearPlayerMember(float _range)
        {
            return GetAllTargets(true, _range).Count > 0;
        }

        protected bool IsNearTeammates(float _range)
        {
            return GetAllTargets(false, _range).Count > 0;
        }
        
        protected bool HasPlayerBlockingRoute()
        {
            var dirToGoal = playerTeamGoal.position - transform.position;
            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, playerTeamGoal.position, 0.2f, dirToGoal, dirToGoal.magnitude);
            
            foreach (RaycastHit hit in hits)
            {
                //if they are running into an enemy character, make them stop at that character and perform melee
                if (!hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    continue;
                }
                
                if (otherCharacter.side.sideGUID != characterBase.side.sideGUID && otherCharacter != characterBase)
                {
                    return true;
                }
            }

            return false;
        }

        protected bool HasPassableTeammate()
        {
            return GetAllTargets(false, characterBase.characterBallManager.shotStrength).Count > 0;
        }
        
        protected bool IsPlayerInDirection(Vector3 direction)
        {
            if (!Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, direction.magnitude, characterCheckMask))
            {
                return false;
            }
            
            hit.transform.TryGetComponent(out PlayableCharacter player);
            return !player.IsNull();
        }

        protected bool IsNearBruiserCharacter(float range)
        {
            var adjustedRange = (range/2) - 0.07f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, adjustedRange, characterCheckMask);

            if (colliders.Length <= 0)
            {
                return false;
            }

            foreach (var _collider in colliders)
            {
                _collider.TryGetComponent(out CharacterBase _characterBase);
                if (!_characterBase)
                {
                    continue;
                }

                if (!_characterBase.characterClassManager.isAbleToReact)
                {
                    continue;
                }

                if (_characterBase.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
        
        protected List<CharacterBase> GetAllTargets(bool isPlayerTeam, float _checkRange)
        {
            var adjustedRange = _checkRange - 0.05f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, adjustedRange, characterCheckMask);
            List<CharacterBase> _targetTransforms = new List<CharacterBase>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    col.TryGetComponent(out CharacterBase _characterBase);

                    if (!_characterBase)
                    {
                        continue;
                    }

                    if (_characterBase == characterBase)
                    {
                        continue;
                    }

                    //Don't do anything to dead or dying characters
                    if (!_characterBase.isAlive)
                    {
                        continue;
                    }
                    
                    if (isPlayerTeam) //Get Player Team Members only
                    {
                        //Is On enemy team
                        if (_characterBase.side.sideGUID == characterBase.side.sideGUID)
                        {
                            continue;
                        }
                        
                        if (!_characterBase.isGoalieCharacter && _characterBase.isTargetable)
                        {
                            _targetTransforms.Add(_characterBase);   
                        }
                    }
                    else // Get AI team Members only
                    {
                        //Not on AI team
                        if (_characterBase.side.sideGUID != characterBase.side.sideGUID)
                        {
                            continue;
                        }
                        
                        if (_characterBase != this.characterBase)
                        {
                            _targetTransforms.Add(_characterBase);
                        }
                    }
                    
                }
            }

            return _targetTransforms;
        }
        
        protected CharacterBase GetHealthiestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }
            
            return _possibleTargets.OrderByDescending(cb => cb.characterLifeManager.currentOverallHealth).LastOrDefault();
        }

        protected CharacterBase GetWeakestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }

            return _possibleTargets.OrderByDescending(cb => cb.characterLifeManager.currentOverallHealth).FirstOrDefault();
        }

        protected bool IsInShootRange()
        {
            var directionToGoal = playerTeamGoal.position - transform.position;
            var distanceToGoal = directionToGoal.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            return enemyMovementThreshold >= distanceToGoal || characterBase.characterBallManager.shotStrength >= distanceToGoal;
        }
        
        protected CharacterAbilityManager.AssignedAbilities GetBestAbility()
        {
            return characterBase.isSilenced ? null : characterBase.characterAbilityManager.GetAssignedAbilities().FirstOrDefault(aa => AbilityIsCurrentlyUsable(aa.ability));
        }

        protected bool HasUsableHealingAbility()
        {
            return !characterBase.isSilenced && characterBase.characterAbilityManager.GetAssignedAbilities().Any(aa =>
                aa.ability.abilityType is AbilityType.ZoneHeal or AbilityType.ApplyStatusSelf
                    or AbilityType.ApplyStatusTarget);
        }

        protected bool HasUsableDamagingAbility()
        {
            return !characterBase.isSilenced && characterBase.characterAbilityManager.GetAssignedAbilities().Any(aa =>
                aa.ability.abilityType is AbilityType.ZoneDamage or AbilityType.DamageCreation
                    or AbilityType.ProjectileDamage or AbilityType.ProjectileKnockback or AbilityType.TrapCreation);
        }

        protected bool AbilityIsCurrentlyUsable(Ability _testAbility)
        {
            switch (_testAbility.abilityType)
            {
                //Currently, never really a bad moment to put on a status or upgrade
                case AbilityType.ApplyStatusSelf or AbilityType.MovementUpgrade or AbilityType.AgilityStatUpgrade
                    or AbilityType.DamageStatUpgrade or AbilityType.ThrowStatUpgrade when characterBase.appliedStatus.IsNull():
                
                    return true;
                
                case AbilityType.ApplyStatusSelf or AbilityType.MovementUpgrade or AbilityType.AgilityStatUpgrade
                    or AbilityType.DamageStatUpgrade or AbilityType.ThrowStatUpgrade:
                    
                    return characterBase.appliedStatus.status.statusType == StatusType.Negative;
                
                case AbilityType.ZoneHeal:
                {
                    if (!IsNearTeammates(_testAbility.range - 0.07f))
                    {
                        return false;
                    }
                    
                    var availableTargets = GetAllTargets(false, _testAbility.range);
                        
                    if(availableTargets.Count == 0){
                        return false;    
                    }
                        
                    return !availableTargets.TrueForAll(cb => cb.characterLifeManager.currentHealthPoints == cb.characterLifeManager.maxHealthPoints);
                }
            }

            //ball is not controlled - relevant abilities: pull, teleport, creations?, 
            if (!TurnController.Instance.ball.isControlled)
            {
                switch (_testAbility.abilityType)
                {
                    case AbilityType.Pull:

                        return IsBallInRange(_testAbility.range - 0.07f);
                    
                    case AbilityType.Teleport: case AbilityType.Dash:
                        
                        if (!IsBallInRange(_testAbility.range - 0.07f))
                        {
                            break;
                        }
                        
                        if (IsNearPlayerMember(enemyMovementRange))
                        {
                            var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                            if(availableTargets.Count == 0){
                                return false;    
                            }

                            foreach (var _target in availableTargets)
                            {
                                if (_target.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                                {
                                    continue;
                                }
                                
                                var dirToBruiser = _target.transform.position - transform.position;
                                
                                return dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius;
                            }
                        }
                        break;
                    
                    case AbilityType.Movement:
                        //never use movement abilities
                        return false;
                    
                    default:
                        return false;
                }
                
                return false;
            }
            
            
            //ball is controlled by other team - relevant abilities: anything that has knockback, knocking ball out of character's hand
            // setting up zones
            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != characterBase.side.sideGUID)
            {
                switch (_testAbility.abilityType)
                {
                    case AbilityType.ProjectileKnockback: case AbilityType.ApplyStatusEnemy: case AbilityType.ProjectileStatus: case AbilityType.ZoneDamage:
                        case AbilityType.ProjectileDamage: case AbilityType.ZoneSelf:
                            
                        //player with ball is in range
                        return IsBallInRange(_testAbility.range - 0.07f);
                    
                    case AbilityType.DamageCreation: case AbilityType.TrapCreation: case AbilityType.WallCreation:
                    
                        return IsNearPlayerMember(_testAbility.range - 0.07f);
                    
                    default:
                        return false;
                }
            }
            
            //ball is controlled by AI TEAM - relevant abilities: dealing damage to anyone around, teleport
            switch (_testAbility.abilityType)
            {
                case AbilityType.Teleport: case AbilityType.Dash:
                    if (!characterBase.characterBallManager.hasBall)
                    {
                        break;
                    }
                    
                    if(!IsBallInRange(_testAbility.range - 0.07f))
                    {
                        break;
                    }
                    
                    if (IsNearPlayerMember(enemyMovementRange))
                    {
                        var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                        if(availableTargets.Count == 0){
                            return false;    
                        }

                        foreach (var _target in availableTargets)
                        {
                            if (_target.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                            {
                                continue;
                            }
                            
                            var dirToBruiser = _target.transform.position - transform.position;
                            return dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius;
                        }
                    }
                    break;
                case AbilityType.ProjectileKnockback: case AbilityType.ApplyStatusEnemy: case AbilityType.ProjectileStatus: case AbilityType.ZoneDamage:
                case AbilityType.ProjectileDamage: case AbilityType.ZoneSelf:
                            
                    //any player nearby
                    return IsNearPlayerMember(_testAbility.range - 0.07f);
                
                default:
                    return false;
            }

            return false;
        }

        #endregion
        
    }
}