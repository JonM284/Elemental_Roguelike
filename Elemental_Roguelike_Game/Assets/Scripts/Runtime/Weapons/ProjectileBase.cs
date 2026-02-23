using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Runtime.Weapons
{
    [RequireComponent(typeof(SphereCollider))]
    public class ProjectileBase: MonoBehaviour
    {

        #region Public Events

        public UnityEvent onProjectileStart;

        public UnityEvent onProjectileEnd;

        #endregion

        #region Actions

        private Action OnEndProjectileMovement;

        #endregion

        #region Serialized Fields

        [SerializeField] private float m_debugRadius = 1f;

        #endregion

        #region Private Fields

        private Vector3 m_velocity;
        
        private float startTime, totalTravelTime;

        private Vector3 m_startPos;

        private Vector3 m_endPos;

        private bool isShot, isAffectWhileMoving, isActualProjectile;

        protected Transform m_user, target;

        private AnimationCurve pathYCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        
        protected Collider[] hitColliders = new Collider[10];
        protected int hitsAmount;

        #endregion

        #region Accessors

        public ProjectileAbilityData projectileAbilityData { get; private set; }

        private int damage => !projectileAbilityData.IsNull()
            ? projectileAbilityData.abilityDamageAmount
            : 0;

        private bool isDamageDealing => damage > 0;

        //private ElementTyping type => m_projectileRef.projectileType;

        private bool armorAffecting => !projectileAbilityData.IsNull() && projectileAbilityData.isAffectArmor;

        private bool hasStatusEffect => !projectileAbilityData.IsNull() && projectileAbilityData.applicableStatusesOnHit.Count > 0;

        private List<StatusData> applicableStatuses => !projectileAbilityData.IsNull()
            ? projectileAbilityData.applicableStatusesOnHit
            : new List<StatusData>();

        #endregion

        #region Unity Events

        void Update()
        {
            if (!isShot)
            {
                return;
            }

            if (isAffectWhileMoving)
            {
                AffectAllInRange();
            }

            var progress = (Time.time - startTime) / totalTravelTime;
            if (progress <= 0.99) {
                m_velocity = Vector3.Lerp(m_startPos, m_endPos, progress);
                m_velocity.y = pathYCurve.Evaluate(progress) + m_startPos.y;
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
            if (!isActualProjectile || projectileAbilityData.IsNull())
            {
                OnEndProjectileMovement?.Invoke();
                OnEndProjectileMovement = null;
                DeleteObject();
                return;
            }
            DoEffect();
        }

        /// <summary>
        /// Real Projectile, deals damage, has target, etc
        /// </summary>
        /// <param name="_info">Scriptable Data</param>
        /// <param name="user">Owner/User of projectile</param>
        /// <param name="_startPos">Start location</param>
        /// <param name="_endPos">End Location</param>
        /// <param name="_target">End Target</param>
        public void Initialize(ProjectileAbilityData _info, Transform user, Vector3 _startPos, 
            Vector3 _endPos, Transform _target)
        {
            if (_info.IsNull())
            {
                Debug.LogError("[Projectile][Initialize] Projectile Data null", gameObject);
                return;
            }
            
            if (projectileAbilityData.IsNull())
            {
                projectileAbilityData = _info;   
            }
            
            startTime = Time.time;
            totalTravelTime = _info.projectileLifetime;
            isAffectWhileMoving = _info.isAffectWhileMoving;
            pathYCurve = _info.animationCurve;
            m_startPos = _startPos;
            transform.position = m_startPos;
            m_endPos = _endPos;
            m_user = user;
            target = _target;
            isActualProjectile = true;
            onProjectileStart?.Invoke();
            isShot = true;
        }

        /// <summary>
        /// This initialize is for fake projectiles.
        /// Projectiles that don't do anything functionally, they are just for display.
        /// </summary>
        public void Initialize(Vector3 startPos, Vector3 endPos, float travelTime, AnimationCurve animationCurve, Action onEndCallback)
        {
            startTime = Time.time;
            totalTravelTime = travelTime;
            isAffectWhileMoving = false;
            pathYCurve = animationCurve;
            m_startPos = startPos;
            transform.position = m_startPos;
            m_endPos = endPos;
            isActualProjectile = false;

            if (!onEndCallback.IsNull())
            {
                OnEndProjectileMovement = onEndCallback;
            }
            
            isShot = true;
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
            if (!isActualProjectile)
            {
                return;
            }
            
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
            if (!isActualProjectile)
            {
                return;
            }
            
            hitsAmount = Physics.OverlapSphereNonAlloc(transform.position, projectileAbilityData.passiveRadius,
                hitColliders, projectileAbilityData.projectileCollisionLayers);

            if (hitsAmount == 0)
            {
                return;
            }

            for (int i = 0; i < hitsAmount; i++)
            {
                if (IsUser(hitColliders[i]))
                {
                    continue;
                }

                hitColliders[i].TryGetComponent(out CharacterBase character);
                
                if(character.IsNull()) continue;
                    
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
            if (projectileAbilityData.applicableStatusesOnHit.Count == 0)
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