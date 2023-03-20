using System;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.Status;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Runtime.Weapons
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileBase: MonoBehaviour
    {

        #region Public Fields

        public UnityEvent onProjectileEnd;

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

        private float moveSpeed => m_info.projectileSpeed;

        private int damage => m_info.projectileDamage;

        private bool isDamageDealing => damage > 0;

        private ElementTyping type => m_info.projectileType;

        private bool armorPiercing => m_info.isArmorPiercing;

        public ProjectileInfo projectileRef => m_info;

        private bool hasStatusEffect => m_info.statusEffect != null;

        private Status.Status statusEffect => m_info.statusEffect;

        private Rigidbody rb => CommonUtils.GetRequiredComponent(ref m_rigidbody, () =>
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
                m_velocity.y = m_info.projectileArcCurve.Evaluate(progress) + m_startPos.y;
            } else {
                m_velocity = m_endPos;
                onProjectileEnd?.Invoke();
                if (isDamageDealing)
                {
                    DealDamage();    
                }
                
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

        public void Initialize(Vector3 _startPos, Vector3 _endPos)
        {
            m_startTime = Time.time;
            m_endTime = Vector3.Magnitude(_startPos - _endPos) / moveSpeed;
            m_startPos = _startPos;
            transform.position = m_startPos;
            m_endPos = _endPos;
        }

        private void DealDamage()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_info.projectileDamageRadius, m_info.projectileCollisionLayers);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var damageable = collider.GetComponent<IDamageable>();
                    damageable?.OnDealDamage(damage, armorPiercing, type);
                    
                    if (hasStatusEffect)
                    {
                        var effectable = collider.GetComponent<IEffectable>();
                        effectable?.ApplyEffect(statusEffect);    
                    }
                    
                }
            }
            
            DeleteObject();
        }

        private void DeleteObject()
        {
            this.ReturnToPool();
        }

        #endregion

        
    }
}