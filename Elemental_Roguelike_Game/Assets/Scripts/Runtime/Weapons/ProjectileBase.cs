using System;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.Status;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Random = UnityEngine.Random;

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

        protected Transform m_user;

        #endregion

        #region Accessors

        public ProjectileInfo m_projectileRef { get; private set; }

        private float moveSpeed => m_projectileRef.projectileSpeed;

        private int damage => m_projectileRef.projectileDamage;

        private bool isDamageDealing => damage > 0;

        private ElementTyping type => m_projectileRef.projectileType;

        private bool armorAffecting => m_projectileRef.isAffectArmor;
        
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

            if (m_projectileRef.isAffectWhileMoving)
            {
                if (hasStatusEffect)
                {
                    AffectAllInRange();
                }
            }

            var progress = (Time.time - m_startTime) / m_endTime;
            if (progress <= 0.99) {
                m_velocity = Vector3.Lerp(m_startPos, m_endPos, progress);
                m_velocity.y = m_projectileRef.projectileArcCurve.Evaluate(progress) + m_startPos.y;
            } else {
                m_velocity = m_endPos;
                OnEndMovement();
            }
            
            transform.position = m_velocity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Wall"))
            {
                OnEndMovement();
            }
        }

        #endregion


        #region Class Implementation

        protected virtual void OnEndMovement()
        {
            onProjectileEnd?.Invoke();
            DoEffect();
        }

        public void Initialize(ProjectileInfo _info, Transform user ,Vector3 _startPos, Vector3 _endPos)
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
            m_user = user;
            onProjectileStart?.Invoke();
        }

        private void DoEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_projectileRef.projectileDamageRadius, m_projectileRef.projectileCollisionLayers);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var damageable = collider.GetComponent<IDamageable>();
                    if (isDamageDealing)
                    {
                        damageable?.OnDealDamage(m_user, damage, !armorAffecting, type, transform ,m_projectileRef.isKnockBack);
                    }
                    else if(damage < 0)
                    {
                        damageable?.OnHeal(damage, armorAffecting);
                    }
                    
                    if (hasStatusEffect)
                    {
                        var randomChanceToHit = Random.Range(0, 100);
                        if (randomChanceToHit <= m_projectileRef.chanceToApplyStatus)
                        {
                            var effectable = collider.GetComponent<IEffectable>();
                            effectable?.ApplyEffect(statusEffect);       
                        }
                    }
                    
                }
            }
            
            DeleteObject();
        }

        private void AffectAllInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_projectileRef.projectileDamageRadius, m_projectileRef.projectileCollisionLayers);

            if (colliders.Length == 0)
            {
                return;
            }

            foreach (var collider in colliders)
            {
                collider.TryGetComponent(out IEffectable effectable);

                if (effectable.IsNull())
                {
                    continue;
                }

                if (effectable.currentStatus != statusEffect)
                {
                    effectable.ApplyEffect(statusEffect);
                }
                
            }
            
        }

        public void ForceStopProjectile()
        {
            OnEndMovement();
        }

        private void DeleteObject()
        {
            isShot = false;
            this.ReturnToPool();
        }

        #endregion

        
    }
}