using System.Collections;
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
    public class ProximityInterceptorCreation: CreationBase, IReactor
    {

        #region Serialized Fields

        [Header("Proximity Interceptor CREATION")]

        [SerializeField] private VFXPlayer localDiscoveryVFX;

        [SerializeField] private GameObject interceptionRangeIndicator;

        [SerializeField] private Transform reactionCameraPoint;

        #endregion

        #region Private Fields
        
        private float m_detonationRadius;
        
        private bool m_isHidden;

        private bool m_isPlayerSide;
        
        private bool m_isWaitingForReactionQueue;

        private ArenaTeamManager m_teamManager;

        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }

        public InterceptorCreationData interceptorCreationData { get; private set; }
        
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
            
            if (IsInRange())
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
            interceptorCreationData = _data as InterceptorCreationData;
            
            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = interceptorCreationData.GetRadius();
            
            m_isHidden = interceptorCreationData.GetIsHidden();
            
            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = interceptorCreationData.GetHiddenLayer();
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

            if (!IsInRange())
            {
                isDoingAction = false;
                return;
            }
            
            StartCoroutine(C_AttemptGrabBall());
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

        private IEnumerator C_AttemptGrabBall()
        {
            ball.SetBallPause(true);
            
            var rollToGrab = interceptorCreationData.GetInterceptionScore();

            bool isPlayer = side == TurnController.Instance.playersSide;
            
            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            var oppositeLayer = isPlayer ? displayEnemyLayerVal : displayLayerVal;
           
            ChangeToVisualLayer(_layer);
            ball.SetVisualsToLayer(oppositeLayer);

            var _LCam = isPlayer ? reactionCameraPoint : ball.ballCamPoint;
            
            var _RCam = isPlayer ? ball.ballCamPoint : reactionCameraPoint;

            int _LValue = isPlayer ? rollToGrab : (int)ball.thrownBallStat;

            int _RValue = isPlayer ? (int)ball.thrownBallStat : rollToGrab;

            yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam ,_LValue, _RValue,  CharacterClass.DEFENDER, isPlayer));

            if (ball.thrownBallStat > rollToGrab)
            {
                //Didn't intercept ball
                Debug.Log($"<color=orange>BIG MISS ON PASS INTERCEPT /// Ball: {ball.thrownBallStat} // Self: {rollToGrab}</color>", this);
                
                ChangeToVisualLayer(charLayerVal);
                ball.SetVisualsToLayer(charLayerVal);
                
                ball.SetBallPause(false);
                HasPerformedReaction();
                yield break;
            }

            ChangeToVisualLayer(charLayerVal);
            ball.SetVisualsToLayer(charLayerVal);
            
            ball.SetBallPause(false);

            //Force ball to come to me
            Debug.Log($"<color=orange>HAS HIT PASS INTERCEPT REACTION /// Ball: {ball.thrownBallStat} // Self: {rollToGrab}</color>", this);
            var dir = transform.position - ball.transform.position;
            ball.ThrowBall(dir, 20f, true, null, 200);
            
            yield return new WaitUntil(() => !ball.isMoving);
        }

        private void ChangeToVisualLayer(LayerMask _preferredLayer)
        {
            creationVisuals.ForEach(g => g.layer = _preferredLayer);
        }

        private bool IsInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_detonationRadius);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    col.TryGetComponent(out BallBehavior ball);
                    if (ball && ball.isThrown)
                    {
                        return true;
                    }
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