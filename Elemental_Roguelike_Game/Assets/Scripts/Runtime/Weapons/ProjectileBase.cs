using System;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Damage;
using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileBase: MonoBehaviour
    {

        #region Events

        private Action OnProjectileEnd;

        #endregion
        
        #region Serialized Fields

        [Header("Info")]
        [SerializeField] private ProjectileInfo m_info; 

        #endregion

        #region Private Fields

        private Vector3 m_velocity;

        private Rigidbody m_rigidbody;

        private float m_endTime;

        private float m_startTime;

        private Vector3 m_startPos;

        private Vector3 m_endPos;

        #endregion

        #region Accessors

        public float moveSpeed => m_info.projectileSpeed;
        
        public int damage => m_info.projectileDamage;

        public ElementTyping type => m_info.projectileType;

        public bool armorPiercing => m_info.isArmorPiercing;

        public ProjectileInfo projectileRef => m_info;

        public Rigidbody rb => CommonUtils.GetRequiredComponent(ref m_rigidbody, () =>
        {
            var r = GetComponent<Rigidbody>();
            return r;
        });

        #endregion

        #region Unity Events

        void Update()
        {
            var progress = (Time.time - m_startTime) / m_endTime;
            if (progress <= 1) {
                m_velocity = Vector3.Lerp(m_startPos, m_endPos, progress);
            } else {
                m_velocity = m_endPos;
                
            }
            
            transform.position = m_velocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall"))
            {
                DeleteObject();
            }
        }

        #endregion


        #region Class Implementation

        public void Initialize(Vector3 _endPos, Action _endCallback = null)
        {
            m_startTime = Time.time;
            m_endTime = m_info.projectileLifetime;
            m_startPos = transform.position;
            m_endPos = _endPos;
            
            if (_endCallback != null)
            {
                OnProjectileEnd = _endCallback;
            }
        }

        public void DealDamage()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_info.projectileDamageRadius, m_info.projectileCollisionLayers);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var damageable = collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.OnDealDamage(damage, armorPiercing, type);
                    }
                }
            }
        }

        public void DeleteObject()
        {
            ProjectileUtils.ReturnToPool(this);
        }

        #endregion

        
    }
}