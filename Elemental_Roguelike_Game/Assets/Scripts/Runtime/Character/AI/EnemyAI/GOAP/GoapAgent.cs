using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using Runtime.Character.AI.EnemyAI.Strategies;
using Runtime.GameControllers;
using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI
{
    [RequireComponent(typeof(CharacterBase))]
    public class GoapAgent: MonoBehaviour
    {

        #region Serialized Fields

        [Header("Sensors")] 
        [SerializeField] private Sensor m_moveRangeSensor;
        [SerializeField] private Sensor m_abilityRangeSensor;
        
        [Header("Layers")]
        [SerializeField] private LayerMask characterCheckMask;
        [SerializeField] private LayerMask obstacleCheckMask;

        #endregion

        #region Private Fields

        private CharacterBase m_characterBase;
        
        private CharacterBase m_targetCharacter;

        private float m_standardWaitTime = 0.3f;

        private float m_abilityWaitTime = 0.2f;

        private bool m_isPerformingAction;

        private AgentGoal m_currentGoal;

        private AgentGoal m_lastGoal;

        private ActionPlan m_actionPlan;

        private AgentAction m_currentAction;

        private Dictionary<string, AgentBelief> m_beliefs = new Dictionary<string, AgentBelief>();

        public HashSet<AgentAction> actions = new();

        private HashSet<AgentGoal> m_goals = new();

        private IGoapPlanner m_goapPlanner = new GoapPlanner();

        #endregion
        
        #region Accessors

        public CharacterBase characterBase =>
            CommonUtils.GetRequiredComponent(ref m_characterBase, GetComponent<CharacterBase>);
        
        public float enemyMovementRange => characterBase.characterMovement.battleMoveDistance - 0.07f;
        
        protected Transform playerTeamGoal => TurnController.Instance.GetPlayerManager().goalPosition;

        protected Transform enemyTeamGoal => TurnController.Instance.GetTeamManager(characterBase.side).goalPosition;

        protected BallBehavior ballReference => TurnController.Instance.ball;
        
        #endregion

        #region Unity Events

        private void Start()
        {
            SetupBeliefs();
            SetupActions();
        }

        #endregion

        #region Class Implementation

        //Conditions: can be thought of as IF statements. This will lead to making decisions about goals -> actions
        private void SetupBeliefs()
        {
            m_beliefs = new Dictionary<string, AgentBelief>();
            BeliefFactor _factory = new BeliefFactor(this, m_beliefs);
            
            _factory.AddBelief("Nothing", () => characterBase.characterActionPoints == 0 || ballReference.isMoving || 
                                                JuiceController.Instance.isDoingActionAnimation || ReactionQueueController.Instance.isDoingReactions);
            _factory.AddBelief("HealthIsLow", () => !characterBase.isAlive);
            _factory.AddBelief("OpponentHasBall", () => TurnController.Instance.ball.isControlled && 
                                                          TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID);
            _factory.AddBelief("TeamHasBall", () => TurnController.Instance.ball.isControlled && 
                                                      TurnController.Instance.ball.controlledCharacterSide.sideGUID == this.characterBase.side.sideGUID && characterBase.heldBall.IsNull());
            _factory.AddBelief("HasAvailableAbilities", () => characterBase.characterAbilityManager.hasAvailableAbility);
            _factory.AddBelief("BallDropped", () => !TurnController.Instance.ball.isControlled);
            _factory.AddBelief("HasBall", () => TurnController.Instance.ball.isControlled && !characterBase.heldBall.IsNull());
            _factory.AddBelief("IsWaitingPassive", () => characterBase.characterClassManager.isCheckingPassive);
            
            _factory.AddLocationBelief("IsNearPlayerGoal", enemyMovementRange * 2, playerTeamGoal);
            _factory.AddLocationBelief("IsNearBallCarrier", enemyMovementRange * 2, TurnController.Instance.ball.currentOwner.transform);
            
            _factory.AddSensorBelief("PlayerInTackleRange", m_moveRangeSensor);
            _factory.AddSensorBelief("TargetInAbilityRange", m_abilityRangeSensor);
        }

        private void SetupActions()
        {
            actions = new HashSet<AgentAction>();
            
            actions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy())
                .AddEffect(m_beliefs["Nothing"])
                .Build()
            );
            
        }

        private void SetupGoals()
        {
            m_goals = new HashSet<AgentGoal>();

            m_goals.Add(new AgentGoal.Builder("Idle")
                .WithPriority(1)
                .WithDesiredEffect(m_beliefs["Nothing"])
                .Build());
            
            m_goals.Add(new AgentGoal.Builder("Take Ball")
                .WithPriority(10)
                .WithDesiredEffect(m_beliefs["Self has ball"])
                .Build());
            
            m_goals.Add(new AgentGoal.Builder("Knock Ball Away")
                .WithPriority(10)
                .WithDesiredEffect(m_beliefs["Ball dropped"])
                .Build());

            m_goals.Add(new AgentGoal.Builder("Overwatch")
                .WithPriority(5)
                .WithDesiredEffect(m_beliefs["Is waiting passive"])
                .Build());
            
            m_goals.Add(new AgentGoal.Builder("Score Goal")
                .WithPriority(100)
                .WithDesiredEffect(m_beliefs["Nothing"])
                .Build());

        }

        private void StartTurn()
        {

            if (m_currentAction == null)
            {
                CalculatePlan();
            }

            if (m_actionPlan != null && m_actionPlan.actions.Count > 0)
            {

                m_currentGoal = m_actionPlan.agentGoal;
                Debug.Log($"Goal: {m_currentGoal.goalName} with {m_actionPlan.actions.Count} actions in plan");
                m_currentAction = m_actionPlan.actions.Pop();
                Debug.Log($"Popped Action: {m_currentAction.actionName}");
                m_currentAction.Start();
                

            }

            if (m_actionPlan != null && m_currentAction != null)
            {
                if (m_currentAction.complete)
                {
                    
                    m_currentAction.Stop();

                    if (m_actionPlan.actions.Count == 0)
                    {
                        Debug.Log("Plan Complete");

                        m_lastGoal = m_currentGoal;

                        m_currentGoal = null;
                    }
                    
                    
                }
            }
            
        }

        private void CalculatePlan()
        {
            var _priorityLevel = m_currentGoal?.goalPriority ?? 0;

            HashSet<AgentGoal> _goalsToCheck = m_goals;

            if (m_currentGoal != null)
            {
                Debug.Log("Current Goal Exists");

                _goalsToCheck = new HashSet<AgentGoal>(m_goals.Where(g => g.goalPriority > _priorityLevel));
            }

            var potentialPlan = m_goapPlanner.Plan(this,_goalsToCheck, m_lastGoal);

            if (potentialPlan != null)
            {
                m_actionPlan = potentialPlan;
            }
        }


        #region Utilities

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

        #endregion

        #region Conditions

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
        protected bool IsCharactersInAbilityRange(bool _isPlayer, float range)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range - 0.07f, characterCheckMask);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    if (col.TryGetComponent(out CharacterBase _character))
                    {
                        if (_isPlayer)
                        {
                            if (_character.side.sideGUID != this.characterBase.side.sideGUID)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (_character.side.sideGUID == this.characterBase.side.sideGUID)
                            {
                                return true;
                            }
                        }
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

        protected bool IsBallInRange(float _range)
        {
            bool inRange = false;
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            if (_range >= distanceToBall)
            {
                inRange = true;
            }

            return inRange;
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
                if (hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    if (otherCharacter.side.sideGUID != characterBase.side.sideGUID && otherCharacter != characterBase)
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

        #endregion
        
        #endregion

    }
}