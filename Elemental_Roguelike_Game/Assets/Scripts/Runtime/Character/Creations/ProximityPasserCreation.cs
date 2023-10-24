using System.Collections;
using System.Collections.Generic;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using Runtime.VFX;
using UnityEngine;

namespace Runtime.Character.Creations
{
    public class ProximityPasserCreation: CreationBase, IReactor
    {
        
        #region Serialized Fields

        [Header("Proximity Interceptor CREATION")]

        [SerializeField] private VFXPlayer localDiscoveryVFX;

        [SerializeField] private GameObject interceptionRangeIndicator;
        
        [SerializeField] private LayerMask characterCheckMask;

        #endregion

        #region Private Fields
        
        private float m_detonationRadius;

        private float m_passStrength;
        
        private bool m_isHidden;

        private bool m_isPlayerSide;
        
        private bool m_isWaitingForReactionQueue;

        private ArenaTeamManager m_teamManager;

        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }

        public PasserCreationData passerCreationData { get; private set; }
        
        private ArenaTeamManager teamManager => CommonUtils.GetRequiredComponent(ref m_teamManager, () =>
        {
            var atm = TurnController.Instance.GetTeamManager(side);
            return atm;
        });

        private BallBehavior ball => TurnController.Instance.ball;
        
        bool IReactor.isPerformingReaction
        {
            get => isDoingAction;
            set => isDoingAction = value;
        }
        
        private LayerMask displayLayerVal => LayerMask.NameToLayer("DISPLAY");
        
        private LayerMask displayEnemyLayerVal => LayerMask.NameToLayer("DISPLAY_ENEMY");

        private LayerMask charLayerVal => LayerMask.NameToLayer("CHARACTER");

        #endregion

        #region Unity Events

        private void Update()
        {
            if (isDoingAction || hasDoneAction)
            {
                return;
            }
            
            if (IsBallDroppedInRange())
            {
                Debug.Log("Should Do Action");
                m_isWaitingForReactionQueue = true;
                ReactionQueueController.Instance.QueueReaction(this, DoAction);
            }
        }

        #endregion

        #region CreationBase Inherited Methods

        public override void Initialize(CreationData _data, Transform _user, CharacterSide _side)
        {
            base.Initialize(_data, _user, _side);
            passerCreationData = _data as PasserCreationData;
            
            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = passerCreationData.GetRadius();
            
            m_isHidden = passerCreationData.GetIsHidden();

            m_passStrength = passerCreationData.GetPassForce();
            
            m_isPlayerSide = _side == TurnController.Instance.playersSide;

            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = passerCreationData.GetHiddenLayer();
                    creationVisuals.ForEach(g => g.layer = hiddenLayer);
                }
            }

            
            //* 2 because it's a radius
            interceptionRangeIndicator.transform.localScale = Vector3.one * (m_detonationRadius * 2);
            
            if (!localSpawnVFX.IsNull())
            {
                localSpawnVFX.Play();
            }

        }

        public override void DoMovementAction()
        {
            //None
        }

        //Fire projectiles at all surrounding enemies
        public override void DoAction()
        {
            isDoingAction = true;
            m_isWaitingForReactionQueue = false;

            if (!IsBallDroppedInRange())
            {
                isDoingAction = false;
                return;
            }
            
            StartCoroutine(C_TryPass());
        }

        public override void CheckTurnPass(CharacterSide _side)
        {
            base.CheckTurnPass(_side);
            if (hasDoneAction)
            {
                DestroyCreation();
            }
        }
        
        #endregion

        #region Class Implementation

        private IEnumerator C_TryPass()
        {
            ball.SetBallPause(true);

            var allAlliesInRange = GetAllTargets(m_isPlayerSide, m_passStrength);

            if (allAlliesInRange.Count == 0)
            {
                Giveup();
                yield break;
            }

            CharacterBase bestPossiblePass = null;

            foreach (var availableAlly in allAlliesInRange)
            {
                if (!bestPossiblePass.IsNull())
                {
                    continue;
                }
                
                var dirToAlly = availableAlly.transform.position - ball.transform.position;
                if (!IsPlayerInDirection(dirToAlly))
                {
                    bestPossiblePass = availableAlly;
                }
            }

            if (bestPossiblePass.IsNull())
            {
                Giveup();
                yield break;
            }
            
            Debug.Log("THROW BALL");
            
            ball.SetBallPause(false);

            var _dirToAlly = bestPossiblePass.transform.position - ball.transform.position;
            ball.ThrowBall(_dirToAlly, m_passStrength, true, null, 90);

            yield return new WaitUntil(() => !ball.isMoving);

            yield return new WaitForSeconds(0.3f);
            
            isDoingAction = false;
            
            DestroyCreation();
        }

        private void Giveup()
        {
            var dir = ball.transform.position - transform.position;
            ball.ThrowBall(dir, m_passStrength, true, null, 90);
        }

        private bool IsBallDroppedInRange()
        {
            var directionToBall = (ball.transform.position - transform.position).FlattenVector3Y();

            if (directionToBall.magnitude < m_detonationRadius)
            {
                if (!ball.isThrown && ball.currentOwner.IsNull())
                {
                    return true;
                }
            }
            
            return false;
        }
        
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
                            _targetTransforms.Add(_character);
                        }
                    }
                    
                }
            }

            return _targetTransforms;
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
        
        private void HasPerformedReaction()
        {
            hasDoneAction = true;
            isDoingAction = false;
        }

        #endregion
    }
}