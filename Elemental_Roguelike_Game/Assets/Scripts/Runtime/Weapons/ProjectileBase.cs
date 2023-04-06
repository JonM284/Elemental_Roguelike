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
    [RequireComponent(typeof(SphereCollider))]
    public class ProjectileBase: MonoBehaviour
    {

        #region Public Fields

        public UnityEvent onProjectileStart;

        public UnityEvent onProjectileEnd;

        #endregion

        #region Private Fields

        private Vector3 m_velocity;
        
        private float m_endTime;

        private float m_startTime;

        private Vector3 m_startPos;

        private Vector3 m_endPos;

        private bool isShot;

        #endregion

        #region Accessors

        public ProjectileInfo m_projectileRef { get; private set; }

        private float moveSpeed => m_projectileRef.projectileSpeed;

        private int damage => m_projectileRef.projectileDamage;

        private bool isDamageDealing => damage > 0;

        private ElementTyping type => m_projectileRef.projectileType;

        private bool armorPiercing => m_projectileRef.isArmorPiercing;
        
        private bool hasStatusEffect => m_projectileRef.statusEffect != null;

        private Status.Status statusEffect => m_projectileRef.statusEffect;

        #endregion

        #region Unity Events

        void Update()
        {
            if (!isShot)
            {
                return;
            }
            var progress = (Time.time - m_startTime) / m_endTime;
            if (progress <= 1) {
                m_velocity = Vector3.Lerp(m_startPos, m_endPos, progress);
                m_velocity.y = m_projectileRef.projectileArcCurve.Evaluate(progress) + m_startPos.y;
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

        public void Initialize(ProjectileInfo _info, Vector3 _startPos, Vector3 _endPos)
        {
            if (m_projectileRef == null)
            {
                m_projectileRef = _info;   
            }
            m_startTime = Time.time;
            m_endTime = Vector3.Magnitude(_startPos - _endPos) / moveSpeed;
            m_startPos = _startPos;
            transform.position = m_startPos;
            m_endPos = _endPos;
            isShot = true;
            onProjectileStart?.Invoke();
        }

        private void DealDamage()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_projectileRef.projectileDamageRadius, m_projectileRef.projectileCollisionLayers);

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
            isShot = false;
            this.ReturnToPool();
        }

        #endregion

        
    }
}