using System;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class BallBehavior : MonoBehaviour
    {

        #region Serialized Field

        [Header("Ball Variables")] [SerializeField]
        private float travelSpeed;

        [Space(15)] [Header("Player Check")] [SerializeField]
        private float playerCheckRadius;

        [SerializeField] private LayerMask playerCheckLayer;

        [Space(15)] [Header("Spring")] [SerializeField]
        private float springConstant;

        [SerializeField] private float damping;

        [Space(15)] [Header("Ground Check")] [SerializeField]
        private GameObject groundIndicator;

        [SerializeField] private float groundIndOffset;

        #endregion

        #region Private Fields

        private Collider m_ballCollider;

        private Rigidbody m_rb;

        private float m_initialY;

        private float m_afterThrowThreshold = 0.25f;

        private float m_currentThrowTime;

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

        public bool isThrown { get; private set; }

        #endregion

        #region Unity Events

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, playerCheckRadius);
        }

        private void Awake()
        {
            m_initialY = transform.position.y;
        }

        private void Update()
        {
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

        public void ThrowBall(Vector3 direction, float throwForce, bool _isThrown)
        {
            m_currentThrowTime = 0;
            isThrown = _isThrown;
            isControlled = false;
            followerTransform = null;
            rb.AddForce(direction.normalized * throwForce, ForceMode.Impulse);
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
            if (m_currentThrowTime > m_afterThrowThreshold)
            {
                CheckForPlayer();
                if (isThrown)
                {
                    isThrown = false;
                }
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
                        isControlled = true;
                        interactable?.PickUpBall(this);
                        groundIndicator.SetActive(!isControlled);
                    }
                }
            }
        }

        public void SetFollowTransform(Transform _follower)
        {
            followerTransform = _follower.transform;
        }
        
        

        #endregion




    }
}