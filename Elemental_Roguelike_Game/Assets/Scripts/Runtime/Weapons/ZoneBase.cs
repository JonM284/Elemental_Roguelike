using System;
using System.Collections;
using Data;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Status;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{

    [RequireComponent(typeof(SphereCollider))]
    public class ZoneBase: MonoBehaviour
    {
        #region Events

        public UnityEvent onZoneStart;

        public UnityEvent onZoneEnd;

        #endregion

        #region Serialized Fields

        [SerializeField] private float m_debugRadius = 1f;

        #endregion

        #region Private Fields

        private CharacterSide m_side;

        private int m_currentRoundTimer;

        private Transform m_user;

        #endregion

        #region Accessors

        public ZoneInfo m_zoneRef { get; private set; }

        private bool hasStatusEffect => !m_zoneRef.IsNull() && !m_zoneRef.statusEffect.IsNull();

        #endregion
        
        #region UnityEvents

        private void OnDrawGizmos()
        {
            if(!m_zoneRef.IsNull()){
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_zoneRef.zoneRadius);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_debugRadius);
            }
        }

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
            TurnController.OnRunEnded += EndZone;
            TurnController.OnResetField += EndZone;
            TurnController.OnBattleEnded += EndZone;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            TurnController.OnRunEnded -= EndZone;
            TurnController.OnResetField -= EndZone;
            TurnController.OnBattleEnded -= EndZone;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!hasStatusEffect)
            {
                return;
            }
            
            if (IsUser(other))
            {
                return;
            }

            //fixes lingering trigger area
            if (m_zoneRef.roundStayAmount == 0)
            {
                return;
            }

            other.TryGetComponent(out IEffectable effectable);
            effectable?.ApplyEffect(m_zoneRef.statusEffect);  
        }

        #endregion

        public void Initialize(ZoneInfo _zoneInfo, Transform _user)
        {
            if (_zoneInfo.IsNull())
            {
                Debug.Log("NO ZONE INFO");
                return;
            }

            m_zoneRef = _zoneInfo;
            
            m_currentRoundTimer = m_zoneRef.roundStayAmount;

            if (!_user.IsNull())
            {
                m_user = _user;
            }
            
            m_side = TurnController.Instance.GetActiveTeamSide();

            onZoneStart?.Invoke();

            //This is an impact only zone
            if(m_zoneRef.roundStayAmount == 0)
            {
                DoEffect();
                StartCoroutine(C_WaitToDoEffect());
            }
        }

        private IEnumerator C_WaitToDoEffect()
        {
            yield return new WaitForSeconds(m_zoneRef.zoneStaySeconds);
            EndZone();
        }

        private void EndZone()
        {
            onZoneEnd?.Invoke();

            this.ReturnToPool();
        }

        
        private void OnChangeActiveTeam(CharacterSide _side)
        {
            if (m_zoneRef.roundStayAmount > 0)
            {
                DoEffect();
            }
            
            if (_side != this.m_side)
            {
                return;
            }

            m_currentRoundTimer--;

            if (m_currentRoundTimer == 0)
            {
                EndZone();
            }
        }

        private void DoEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_zoneRef.zoneRadius, m_zoneRef.zoneCheckLayer);
            
            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {

                    if (m_zoneRef.isIgnoreUser)
                    {
                        if (IsUser(collider))
                        {
                            continue;
                        }
                    }

                    if (m_zoneRef.isStopReaction)
                    {
                        collider.TryGetComponent(out CharacterClassManager classManager);
                        if (!classManager.IsNull())
                        {
                            classManager.SetAbleToReact(false);
                        }
                    }

                    var damageable = collider.GetComponent<IDamageable>();
                    if (m_zoneRef.zoneDamage > 0)
                    {
                        damageable?.OnDealDamage(m_user, m_zoneRef.zoneDamage, !m_zoneRef.isArmorAffecting,
                            m_zoneRef.elementType, transform, m_zoneRef.hasKnockback);
                    }
                    else if(m_zoneRef.zoneDamage < 0)
                    {
                        damageable?.OnHeal(m_zoneRef.zoneDamage, m_zoneRef.isArmorAffecting);
                    }
                    
                    if (m_zoneRef.isRandomKnockawayBall)
                    {
                        collider.TryGetComponent(out IBallInteractable ballInteractable);
                        if (!ballInteractable.IsNull())
                        {
                            ballInteractable.KnockBallAway(null);
                        }
                    }

                    if (hasStatusEffect)
                    {
                        collider.TryGetComponent(out IEffectable effectable);
                        effectable?.ApplyEffect(m_zoneRef.statusEffect);   
                    }
                    
                }
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


    }
}