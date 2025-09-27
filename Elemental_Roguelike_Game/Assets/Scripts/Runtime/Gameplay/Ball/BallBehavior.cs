using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.Gameplay
{
    public class BallBehavior : MonoBehaviour
    {

        #region Serialized Field

        [Header("Ball Variables")] [SerializeField]
        private float m_wallRayLegnth = 0.3f;

        [SerializeField] private LayerMask wallLayers;

        [SerializeField] private float m_fullDecelTime = 1.9f;

        [Space(15)] 
        [Header("Player Check")] [SerializeField] private float playerCheckRadius;

        [SerializeField] private LayerMask playerCheckLayer, outsideInfluenceLayers;

        [Space(15)] [Header("Spring")] [SerializeField]
        private float springConstant;

        [SerializeField] private float damping;

        [Space(15)] [Header("Ground Check")] [SerializeField]
        private GameObject groundIndicator;

        [SerializeField] private float groundIndOffset;

        [SerializeField] private Transform m_ballCamPoint;

        [Space(15)] [Header("Visuals")] [SerializeField]
        private List<GameObject> visualGOs = new List<GameObject>();

        [SerializeField] private Canvas ballLevelCanvas;
        [SerializeField] private GameObject ballThrowVisuals;
        [SerializeField] private Image ballThrowIntensityImage;
        [SerializeField] private TMP_Text ballThrowIntensityText;
        [SerializeField] private float countFPS = 30f, textDuration = 0.2f;

        #endregion

        #region Private Fields

        private List<CharacterBase> m_lastContactedCharacters = new List<CharacterBase>();

        private Collider m_ballCollider;

        private Rigidbody m_rb;

        private float m_initialY;

        private float m_tempDragSpeed;

        private float m_afterThrowThreshold = 0.25f;

        private float m_currentThrowTime;

        private float currentBallForce;

        private Vector3 ballThrownDirection, ballOutsideInfluence;

        private bool m_isBallPaused;

        private Vector3 m_ballStartPosition;

        private float maxIntensity = 30f;

        private Collider[] foundPlayers = new Collider[15];
        private int foundPlayersAmount;

        private Dictionary<CharacterBase, Vector3> currentInfluences = new Dictionary<CharacterBase, Vector3>();

        #endregion

        #region Accessors

        private Collider ballCollider => CommonUtils.GetRequiredComponent(ref m_ballCollider, () =>
        {
            var c = GetComponent<Collider>();
            return c;
        });

        private Rigidbody rb => CommonUtils.GetRequiredComponent(ref m_rb, () =>
        {
            var r = GetComponent<Rigidbody>();
            return r;
        });

        public bool isControlled { get; private set; }

        public Transform followerTransform { get; private set; }

        public CharacterBase currentOwner { get; private set; }

        public Transform ballCamPoint => m_ballCamPoint;

        public bool isThrown { get; private set; }

        public bool canBeCaught { get; private set; }

        public bool isMoving => currentBallForce > 0;
        
        public float thrownBallStat { get; private set; }

        public List<CharacterBase> lastContactedCharacters => m_lastContactedCharacters;

        public CharacterBase lastThrownCharacter { get; private set; }

        public CharacterBase lastHeldCharacter { get; private set; }

        public CharacterSide controlledCharacterSide { get; private set; }

        #endregion

        #region Unity Events

        private void Awake()
        {
            m_initialY = transform.position.y;
            m_ballStartPosition = transform.position;
            ballLevelCanvas.worldCamera = CameraUtils.GetMainCamera();
        }

        private void Update()
        {
            if (m_isBallPaused)
            {
                return;
            }
            
            if (isControlled)
            {
                FollowTransform();
            }
            else
            {
                FreeBall();
            }
        }

        #endregion
        
        #region Class Implementation

        public void ThrowBall(Vector3 direction, float throwForce, bool _isThrown, CharacterBase _character, int _thrownBallStat)
        {
            thrownBallStat = _thrownBallStat;
            m_currentThrowTime = 0;
            isThrown = _isThrown;
            
            if (isThrown && !_character.IsNull())
            {
                lastThrownCharacter = _character;
            }

            lastHeldCharacter = _character;
            isControlled = false;
            controlledCharacterSide = null;
            followerTransform = null;

            currentOwner = null;
            ballThrownDirection = direction;
            currentBallForce = throwForce;
            m_tempDragSpeed = (throwForce)/m_fullDecelTime;
            
            CameraUtils.SetCameraTrackPos(transform, true);
            ballThrowVisuals.SetActive(true);
        }

        private void FollowTransform()
        {
            transform.position = followerTransform.position;
        }

        private void FreeBall()
        {
            if (isControlled)
            {
                return;
            }
            
            BouncyFloat();
            MarkGround();

            if (currentBallForce > 0)
            {
                if (Physics.Raycast(transform.position, ballThrownDirection, out RaycastHit hit, m_wallRayLegnth, wallLayers))
                {
                    ballThrownDirection = Vector3.Reflect(ballThrownDirection, hit.normal);
                }
                
                currentBallForce -= m_tempDragSpeed * Time.deltaTime;
                thrownBallStat -= m_tempDragSpeed * Time.deltaTime;
                var ballVelocity = GetCurrentInfluenceDirection().normalized * (currentBallForce * Time.deltaTime);
                Debug.DrawRay(transform.position, ballThrownDirection, Color.green, 30f);
                //Debug.DrawRay(transform.position, ballVelocity, Color.green, 30f);
                UpdateThrowIntensityVisuals();
                rb.MovePosition(rb.position + ballVelocity);
                ballThrownDirection = ballVelocity.normalized * currentBallForce;
                Debug.DrawRay(transform.position, ballThrownDirection, Color.red, 30f);
            }else
            {
                if (isThrown)
                {
                    isThrown = false;
                    thrownBallStat = 0;
                    lastThrownCharacter = null;
                    ballThrowVisuals.SetActive(false);
                    currentInfluences.Clear();
                }   
            }
            
            if (m_currentThrowTime > m_afterThrowThreshold)
            {
                if (!canBeCaught)
                {
                    canBeCaught = true;
                }
                CheckForPlayer();
            }
            else
            {
                if (canBeCaught)
                {
                    canBeCaught = false;
                }
                m_currentThrowTime += Time.deltaTime;
            }

        }

        private void BouncyFloat()
        {
            float displacement = m_initialY - transform.position.y;
            float springForce = springConstant * displacement;
            float dampingForce = -damping * rb.linearVelocity.y;
            float totalForce = springForce + dampingForce;
        
            rb.AddForce(Vector3.up * totalForce, ForceMode.Force);
        }

        private void MarkGround()
        {
            if (isControlled)
            {
                return;
            }
            
            groundIndicator.SetActive(!isControlled);

            groundIndicator.transform.position =
                new Vector3(transform.position.x, groundIndOffset, transform.position.z);
        }

        private void CheckForPlayer()
        {
            foundPlayersAmount = Physics.OverlapSphereNonAlloc(transform.position, playerCheckRadius, foundPlayers ,playerCheckLayer);

            if (foundPlayersAmount == 0)
            {
                return;
            }
            
            foreach (var collider in foundPlayers)
            {
                collider.TryGetComponent(out CharacterBase _character);
                    
                if (_character.IsNull())
                {
                    continue;
                }
                    
                if (!_character.characterBallManager.canPickupBall)
                {
                    continue;
                }

                if (!lastThrownCharacter.IsNull() && _character == lastThrownCharacter)
                {
                    continue;
                }
                    
                isControlled = true;
                currentBallForce = 0;
                    
                if (isThrown)
                {
                    isThrown = false;
                    lastThrownCharacter = null;
                }
                    
                controlledCharacterSide = _character.side;
                    
                _character.PickUpBall(this);
                    
                groundIndicator.SetActive(!isControlled);
                break;
            }
        }

        public async UniTask ChangeBallIntensity(CharacterSide side, float amountChange)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Token.ThrowIfCancellationRequested();

            if (side.sideGUID != lastThrownCharacter.side.sideGUID)
            {
                return;
            }
            
            SetBallPause(true);
            var ballForce = Mathf.Clamp(currentBallForce + (amountChange), 0, maxIntensity);
            await CountToNumberAsync(ballForce, cts.Token);

            SetBallPause(false);
        }
        
        private async UniTask CountToNumberAsync(float _newValue, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            float _previousValue = currentBallForce;
            int _stepAmount;
            float _waitTime = 1 / countFPS;

            _stepAmount = _newValue - _previousValue  < 0 ? 
                Mathf.FloorToInt((_newValue - _previousValue) / (countFPS * textDuration)) 
                : Mathf.CeilToInt((_newValue - _previousValue) / (countFPS * textDuration));

            _stepAmount = Mathf.Abs(_stepAmount);
            
            //Going up
            if (_previousValue < _newValue)
            {
                while (_previousValue < _newValue)
                {
                    _previousValue += _stepAmount;
                    if (_previousValue > _newValue)
                    {
                        _previousValue = _newValue;
                    }
                    
                    ballThrowIntensityText.text = _previousValue.ToString();
                    ballThrowIntensityImage.fillAmount = _previousValue / maxIntensity;
                    await UniTask.WaitForSeconds(_waitTime, cancellationToken:token);
                }
            }
            else //Going down
            {
                while (_previousValue > _newValue)
                {
                    _previousValue -= _stepAmount;
                    if (_previousValue < _newValue)
                    {
                        _previousValue = _newValue;
                    }
                    
                    ballThrowIntensityText.text = _previousValue.ToString();
                    ballThrowIntensityImage.fillAmount = _previousValue / maxIntensity;
                    await UniTask.WaitForSeconds(_waitTime, cancellationToken:token);
                }
            }

            currentBallForce = _newValue;
        }
        
        public void AddInfluence(CharacterBase influencingCharacter, Vector3 newInfluenceDirection)
        {
            if (!isThrown)
            {
                return;
            }

            if (influencingCharacter == lastThrownCharacter)
            {
                return;
            }

            if (currentInfluences.TryAdd(influencingCharacter, newInfluenceDirection)) return;
            currentInfluences[influencingCharacter] = newInfluenceDirection;
        }
        
        public void RemoveInfluence(CharacterBase removingCharacter)
        {
            if (!currentInfluences.ContainsKey(removingCharacter))
            {
                return;
            }
            Debug.Log("<color=orange>Removing Influence</color>");
            currentInfluences.Remove(removingCharacter);
        }

        private void UpdateThrowIntensityVisuals()
        {
            ballThrowIntensityImage.fillAmount = currentBallForce / maxIntensity;
            ballThrowIntensityText.text = Mathf.CeilToInt(currentBallForce).ToString();
        }
        
        private Vector3 GetCurrentInfluenceDirection()
        {
            if (currentInfluences.Count == 0)
            {
                return ballThrownDirection;
            }
            
            return new Vector3(ballThrownDirection.x + currentInfluences.Values.Sum(vec => vec.x),
                0,
                ballThrownDirection.z + currentInfluences.Values.Sum(vec => vec.z));
        }

        /// <summary>
        /// AKA, pickup ball
        /// </summary>
        public void SetFollowTransform(Transform _follower, CharacterBase _ownerCharacter)
        {
            followerTransform = _follower.transform;
            currentOwner = _ownerCharacter;
            
            ballThrowVisuals.SetActive(false);
            currentInfluences.Clear();
            
            if (m_lastContactedCharacters.Count >= 5)
            {
                m_lastContactedCharacters.RemoveAt(0);
            }
            m_lastContactedCharacters.Add(currentOwner);
        }


        public void SetBallPause(bool _isPaused)
        {
            m_isBallPaused = _isPaused;
            rb.linearVelocity = Vector3.zero;
        }

        public void ForceStopBall()
        {
            currentBallForce = 0;
            rb.linearVelocity = Vector3.zero;
        }

        public void ReduceForce(int _reductionAmount)
        {
            currentBallForce -= _reductionAmount;
        }

        public void ResetBall()
        {
            ForceStopBall();
            if (isControlled)
            {
                currentOwner.DetachBall();
                m_lastContactedCharacters.Clear();
                currentOwner = null;
            }
            isControlled = false;
            followerTransform = null;
            transform.position = m_ballStartPosition;
            currentInfluences.Clear();
        }

        public void SetVisualsToLayer(LayerMask _layer)
        {
            visualGOs.ForEach(g => g.layer = _layer);
        }


        #endregion




    }
}