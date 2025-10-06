using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using Data.AbilityDatas;
using Data.Elements;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character;
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

        #region Serialized Fields

        [SerializeField] private float m_debugRadius = 1f;

        #endregion

        #region Private Fields

        private Vector3 m_velocity;
        
        private float startTime, totalTravelTime;

        private Vector3 m_startPos;

        private Vector3 m_endPos;

        private bool isShot;

        protected Transform m_user, target;
        
        protected Collider[] hitColliders = new Collider[10];
        protected int hitsAmount;

        #endregion

        #region Accessors

        public ProjectileAbilityData projectileAbilityData { get; private set; }
        
        private int damage => projectileAbilityData.abilityDamageAmount;

        private bool isDamageDealing => damage > 0;

        //private ElementTyping type => m_projectileRef.projectileType;

        private bool armorAffecting => projectileAbilityData.isAffectArmor;
        
        private bool hasStatusEffect => projectileAbilityData.applicableStatusesOnHit.Count > 0;

        private List<StatusData> applicableStatuses => projectileAbilityData.applicableStatusesOnHit;

        #endregion

        #region Unity Events

        void Update()
        {
            if (!isShot)
            {
                return;
            }

            if (projectileAbilityData.isAffectWhileMoving)
            {
                AffectAllInRange();
            }

            var progress = (Time.time - startTime) / totalTravelTime;
            if (progress <= 0.99) {
                m_velocity = Vector3.Lerp(m_startPos, m_endPos, progress);
                m_velocity.y = projectileAbilityData.animationCurve.Evaluate(progress) + m_startPos.y;
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
            if (!isShot)
            {
                return;
            }
            
            onProjectileEnd?.Invoke();
            DoEffect();
        }

        public void Initialize(ProjectileAbilityData _info, Transform user, Vector3 _startPos, 
            Vector3 _endPos, Transform _target)
        {
            if (projectileAbilityData.IsNull())
            {
                projectileAbilityData = _info;   
            }
            
            startTime = Time.time;
            totalTravelTime = projectileAbilityData.projectileLifetime;
            m_startPos = _startPos;
            transform.position = m_startPos;
            m_endPos = _endPos;
            isShot = true;
            m_user = user;
            target = _target;
            onProjectileStart?.Invoke();
        }

        private void DoEffect()
        {
            if (projectileAbilityData.projectileEndType == ProjectileEndType.EXPLODE)
            {
                CheckSurroundingExplosion();
            }
            else
            {
                DamageSelectedTarget();
            }
            
            DeleteObject();
        }

        private void DamageSelectedTarget()
        {
            if (target.IsNull())
            {
                return;
            }
            
            target.TryGetComponent(out CharacterBase _character);
            
            EffectTargetCharacter(_character);
        }

        private void CheckSurroundingExplosion()
        {
            hitsAmount = Physics.OverlapSphereNonAlloc(transform.position, projectileAbilityData.passiveRadius,
                hitColliders , projectileAbilityData.projectileCollisionLayers);

            if (hitsAmount < 0)
            {
                return;
            }

            for (int i = 0; i < hitsAmount; i++)
            {
                if (IsUser(hitColliders[i]))
                {
                    continue;
                }

                hitColliders[i].TryGetComponent(out CharacterBase _character);
                
                if(_character.IsNull()) continue;
                    
                EffectTargetCharacter(_character);
            }
        }

        private void EffectTargetCharacter(CharacterBase target)
        {
            if (projectileAbilityData.isStopReaction)
            {
                target.SetCharacterUsable(false);
                target.characterClassManager.SetAbleToReact(false);
            }

            if (isDamageDealing)
            {
                target.OnDealDamage(m_user, damage, !armorAffecting, null, 
                    transform ,projectileAbilityData.abilityKnockbackAmount > 0);
            }
            else if(damage < 0)
            {
                target.OnHeal(damage, armorAffecting);
            }

            if (!hasStatusEffect)
            {
                return;
            }
                
            foreach (var status in projectileAbilityData.applicableStatusesOnHit)
            {
                target.ApplyStatus(status).Forget();  
            }
        }

        private void AffectAllInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, projectileAbilityData.passiveRadius , projectileAbilityData.projectileCollisionLayers);

            if (colliders.Length == 0)
            {
                return;
            }

            foreach (var collider in colliders)
            {
                if (IsUser(collider))
                {
                    continue;
                }
             
                collider.TryGetComponent(out CharacterBase character);

                if (character.IsNull())
                {
                    return;
                }
                
                ApplyStatus(character);
            }
            
        }
        
        private bool IsUser(Collider _collider)
        {
            if (_collider.IsNull())
            {
                return false;
            }

            if (m_user.IsNull())
            {
                Debug.Log("USER NULL");
                return false;
            }

            if (_collider.transform == m_user)
            {
                return true;
            }


            return false;
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
        
        private void ApplyStatus(CharacterBase _character)
        {
            if (projectileAbilityData.applicableStatusesOnHit.Count <= 0)
            {
                return;
            }
            
            foreach (var _statusData in projectileAbilityData.applicableStatusesOnHit)
            {
                if(_character.IsNull() || _character.ContainsStatus(_statusData)){
                    continue;   
                }

                _character.ApplyStatus(_statusData).Forget();
            }
        }

        #endregion

        
    }
}