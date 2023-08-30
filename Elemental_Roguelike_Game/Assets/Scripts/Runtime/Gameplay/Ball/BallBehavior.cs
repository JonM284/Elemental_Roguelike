using System;
using System.Collections.Generic;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runtime.Gameplay
{
    public class BallBehavior : MonoBehaviour
    {

        #region Serialized Field

        [Header("Ball Variables")]
        [SerializeField] private float m_dragSpeed = 4;

        [SerializeField] private float m_wallRayLegnth = 0.3f;

        [SerializeField] private LayerMask wallLayers;
        
        [Space(15)] [Header("Player Check")] [SerializeField]
        private float playerCheckRadius;

        [SerializeField] private LayerMask playerCheckLayer;

        [Space(15)] [Header("Spring")] [SerializeField]
        private float springConstant;

        [SerializeField] private float damping;

        [Space(15)] [Header("Ground Check")] [SerializeField]
        private GameObject groundIndicator;

        [SerializeField] private float groundIndOffset;

        [SerializeField] private Transform m_ballCamPoint;

        [Space(15)] [Header("Visuals")] [SerializeField]
        private List<GameObject> visualGOs = new List<GameObject>();

        #endregion

        #region Private Fields

        private List<CharacterBase> m_lastContactedCharacters = new List<CharacterBase>();

        private Collider m_ballCollider;

        private Rigidbody m_rb;

        private float m_initialY;

        private float m_afterThrowThreshold = 0.25f;

        private float m_currentThrowTime;

        private float m_currentBallForce;

        private Vector3 m_ballThrownDirection;

        private bool m_isBallPaused;

        private Vector3 m_ballStartPosition;

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

        public float thrownBallStat { get; private set; }

        public List<CharacterBase> lastContactedCharacters => m_lastContactedCharacters;

        public CharacterSide lastThrownCharacterSide { get; private set; }

        public CharacterSide controlledCharacterSide { get; private set; }

        #endregion

        #region Unity Events

        private void Awake()
        {
            m_initialY = transform.position.y;
            m_ballStartPosition = transform.position;
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

        public void ThrowBall(Vector3 direction, float throwForce, bool _isThrown, CharacterSide _characterSide, int _thrownBallStat)
        {
            thrownBallStat = _thrownBallStat;
            m_currentThrowTime = 0;
            isThrown = _isThrown;
            if (isThrown)
            {
                lastThrownCharacterSide = _characterSide;
            }
            isControlled = false;
            controlledCharacterSide = null;
            followerTransform = null;

            currentOwner = null;
            m_ballThrownDirection = direction;
            m_currentBallForce = throwForce;
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

            if (m_currentBallForce > 0)
            {
                if (Physics.Raycast(transform.position, m_ballThrownDirection, out RaycastHit hit, m_wallRayLegnth, wallLayers))
                {
                    m_ballThrownDirection = Vector3.Reflect(m_ballThrownDirection, hit.normal);
                }
                
                m_currentBallForce -= m_dragSpeed * Time.deltaTime;
                thrownBallStat -= m_dragSpeed * Time.deltaTime;
                var ballVelocity = m_ballThrownDirection.normalized * (m_currentBallForce * Time.deltaTime);
                rb.MovePosition(rb.position + ballVelocity);
            }else if (m_currentBallForce <= 0)
            {
                if (isThrown)
                {
                    isThrown = false;
                    thrownBallStat = 0;
                }   
            }
            
            if (m_currentThrowTime > m_afterThrowThreshold)
            {
                CheckForPlayer();
            }
            else
            {
                m_currentThrowTime += Time.deltaTime;
            }

        }

        private void BouncyFloat()
        {
            float displacement = m_initialY - transform.position.y;
            float springForce = springConstant * displacement;
            float dampingForce = -damping * rb.velocity.y;
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, playerCheckRadius, playerCheckLayer);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var interactable = collider.GetComponent<IBallInteractable>();
                    if (!interactable.IsNull())
                    {
                        if (interactable is CharacterBase characterBase)
                        {
                            if (!characterBase.canPickupBall)
                            {
                                return;
                            }
                            isControlled = true;
                            if (isThrown)
                            {
                                m_currentBallForce = 0;
                                isThrown = false;
                            }
                            controlledCharacterSide = characterBase.side;
                            interactable?.PickUpBall(this);
                            groundIndicator.SetActive(!isControlled);
                        }
                    }
                }
            }
        }

        public void SetFollowTransform(Transform _follower, CharacterBase _ownerCharacter)
        {
            followerTransform = _follower.transform;
            currentOwner = _ownerCharacter;
            
            if (m_lastContactedCharacters.Count >= 5)
            {
                m_lastContactedCharacters.RemoveAt(0);
            }
            m_lastContactedCharacters.Add(currentOwner);
        }


        public void SetBallPause(bool _isPaused)
        {
            m_isBallPaused = _isPaused;
            rb.velocity = Vector3.zero;
        }

        public void ForceStopBall()
        {
            m_currentBallForce = 0;
            rb.velocity = Vector3.zero;
        }

        public void ReduceForce(int _reductionAmount)
        {
            m_currentBallForce -= _reductionAmount;
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
        }

        public void SetVisualsToLayer(LayerMask _layer)
        {
            visualGOs.ForEach(g => g.layer = _layer);
        }


        #endregion




    }
}