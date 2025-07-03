using System;
using Cysharp.Threading.Tasks;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Character
{
    public class CharacterBallManager: MonoBehaviour, IBallInteractable
    {

        #region Read-Only

        private readonly string handPosTransformName = "BallHoldPos@Transform";

        #endregion
        
        #region Actions

        public static event Action<CharacterBase> BallPickedUp;

        public static event Action<CharacterBase> BallThrown;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private float m_shotDistanceBase = 1f;

        [SerializeField] private float m_maxShotDistance = 6f;

        [SerializeField] private Transform m_handPos;
        
        [SerializeField] protected GameObject ballOwnerIndicator;

        [SerializeField] protected LineRenderer ballThrowIndicator;

        [SerializeField] private LayerMask goalLayer;

        #endregion

        #region Private Fields

        private CharacterBase m_characterBase;

        private int shootingScore, passingScore;
        private int shootingScoreOriginal, passingScoreOriginal;

        private int highestPossibleScore = 100;
        private BallBehavior m_heldBall;
        private bool m_canPickupBall = true;

        #endregion

        #region Accessors
        
        public float shotStrength => m_shotDistanceBase + (shootingScore/100f) * m_maxShotDistance;

        public float passStrength => m_shotDistanceBase + (passingScore / 100f) * m_maxShotDistance;
        
        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, GetComponent<CharacterBase>);

        public bool hasBall => !m_heldBall.IsNull();

        public bool canPickupBall => m_canPickupBall;
        
        #endregion

        #region IBallInteractalbe Inherited Methods

        BallBehavior IBallInteractable.heldBall
        {
            get => m_heldBall;
            set => m_heldBall = value;
        }

        bool IBallInteractable.canPickupBall
        {
            get => m_canPickupBall;
            set => m_canPickupBall = value;
        }

        public void PickUpBall(BallBehavior ball)
        {
            if (!m_canPickupBall)
            {
                return;
            }
            
            m_heldBall = ball;
            m_heldBall.SetFollowTransform(m_handPos, characterBase);
            ballOwnerIndicator.SetActive(true);
            BallPickedUp?.Invoke(characterBase);
        }

        public void DetachBall()
        {
            m_heldBall = null;
            ballOwnerIndicator.SetActive(false);
        }

        public void KnockBallAway(Transform attacker = null)
        {
            if (m_heldBall.IsNull())
            {
                return;
            }

            var randomPos = Random.insideUnitCircle;
            
            var direction = attacker.IsNull() ? 
                (transform.position + new Vector3(randomPos.x, 0 , randomPos.y) 
                - transform.position) : attacker.position - transform.position;
            
            Debug.Log(shotStrength);
            
            m_heldBall.ThrowBall(direction, shotStrength, false, this.characterBase, 0);
            
            m_heldBall = null;
            
            ballOwnerIndicator.SetActive(false);
        }

        public void ThrowBall(Vector3 _position, bool _isShot)
        {
            if (m_heldBall.IsNull())
            {
                return;
            }
            
            var _ballThrowSpeed = _isShot ? shotStrength : passStrength;
            var _attachedStat = _isShot
                ? GetRandomShootStat()
                : GetRandomPassStat();
            
            m_heldBall.ThrowBall(_position - transform.position, 
                _ballThrowSpeed,
                true, this.characterBase,
                _attachedStat);
            
            m_heldBall = null;
            ballOwnerIndicator.SetActive(false);
            
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(false);
            }
            
            BallThrown?.Invoke(characterBase);
        }

        #endregion

        #region Class Implementation


        public async UniTask Initialize(CharacterStatsBase _characterStats, Transform ballHandPos)
        {
            if (_characterStats.IsNull())
            { 
                return;   
            }

            shootingScoreOriginal = _characterStats.shootingScore;
            shootingScore = shootingScoreOriginal;
            passingScoreOriginal = _characterStats.passingScore;
            passingScore = passingScoreOriginal;
            
            m_handPos = ballHandPos;

            m_canPickupBall = true;
            
            await UniTask.WaitForEndOfFrame();
        }
        
        public void MarkThrowBall(Vector3 _position)
        {
            if (ballThrowIndicator.IsNull())
            {
                return;
            }
            
            //ToDo: update points if the player is aiming at a wall
            bool _isShot = IsShot(_position);
            var _throwingStrength = _isShot ? shotStrength : passStrength;
            
            var dir = transform.InverseTransformDirection(_position - transform.position);
            var _decelTime = 1.9f;
            var _decel = (_throwingStrength)/_decelTime;
            var _dist = (0.5f * _decel) * (_decelTime * _decelTime);
            var furthestPoint = (dir.normalized * _dist);

            ballThrowIndicator.SetPosition(1, new Vector3(furthestPoint.x, furthestPoint.z, 0));
        }

        public void TransferBall(CharacterBase _ballHolder, CharacterBase _ballStealer)
        {
            var _ball = TurnController.Instance.ball;
            _ballHolder.DetachBall();
            _ballStealer.PickUpBall(_ball);
        }

        public bool IsShot(Vector3 _endPos)
        {
            var _dir = _endPos - transform.position;
            var strongerStrength = shotStrength > passStrength ? shotStrength : passStrength;
            var _furthestPoint = transform.position + (_dir.normalized * strongerStrength);
            var _mag = _furthestPoint - transform.position;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, _dir, _mag.magnitude, goalLayer, QueryTriggerInteraction.Collide);

            if (hits.Length == 0)
            {
                Debug.DrawLine(transform.position, _furthestPoint, Color.red);
                return false;
            }
            
            Debug.DrawLine(transform.position, _furthestPoint, Color.green);
            return true;
            
        }

        public void SetCanPickupBall(bool _canPickupBall)
        {
            m_canPickupBall = _canPickupBall;
        }

        public void DisplayThrowIndicator(bool _isOn)
        {
            ballThrowIndicator.gameObject.SetActive(_isOn);
        }
        
        
        private int GetRandomShootStat()
        {
            return Random.Range(shootingScore, highestPossibleScore);
        }

        private int GetRandomPassStat()
        {
            return Random.Range(passingScore, highestPossibleScore);
        }

        #endregion
    }
}