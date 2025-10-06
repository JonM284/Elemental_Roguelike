using System;
using System.Collections;
using Data;
using Data.AbilityDatas;
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

        public AoeZoneData MAoeZoneRef { get; private set; }

        private bool hasStatusEffect => !MAoeZoneRef.IsNull() && !MAoeZoneRef.statusEntityBaseEffect.IsNull();

        #endregion
        
        #region UnityEvents

        private void OnDrawGizmos()
        {
            if(!MAoeZoneRef.IsNull()){
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, MAoeZoneRef.zoneRadius);
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
            if (MAoeZoneRef.roundStayAmount == 0)
            {
                return;
            }

            other.TryGetComponent(out IEffectable effectable);
            //ToDo: Status
            //effectable?.ApplyEffect(m_zoneRef.statusBaseEffect);  
        }

        #endregion

        public void Initialize(AoeZoneData aoeZoneData, Transform _user)
        {
            if (aoeZoneData.IsNull())
            {
                Debug.Log("NO ZONE INFO");
                return;
            }

            MAoeZoneRef = aoeZoneData;
            
            m_currentRoundTimer = MAoeZoneRef.roundStayAmount;

            if (!_user.IsNull())
            {
                m_user = _user;
            }
            
            m_side = TurnController.Instance.GetActiveTeamSide();

            onZoneStart?.Invoke();

            //This is an impact only zone
            if(MAoeZoneRef.roundStayAmount == 0)
            {
                DoEffect();
                StartCoroutine(C_WaitToDoEffect());
            }
        }

        private IEnumerator C_WaitToDoEffect()
        {
            yield return new WaitForSeconds(1f);
            EndZone();
        }

        private void EndZone()
        {
            onZoneEnd?.Invoke();

            this.ReturnToPool();
        }

        
        private void OnChangeActiveTeam(CharacterSide _side)
        {
            if (MAoeZoneRef.roundStayAmount > 0)
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, MAoeZoneRef.zoneRadius, MAoeZoneRef.zoneCheckLayer);
            
            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {

                    if (MAoeZoneRef.isIgnoreUser)
                    {
                        if (IsUser(collider))
                        {
                            continue;
                        }
                    }

                    if (MAoeZoneRef.isStopReaction)
                    {
                        collider.TryGetComponent(out CharacterBase _character);
                        if (_character)
                        {
                            _character.SetCharacterUsable(false);
                            _character.characterClassManager.SetAbleToReact(false);
                        }
                    }

                    var damageable = collider.GetComponent<IDamageable>();
                    if (MAoeZoneRef.zoneDamage > 0)
                    {
                        damageable?.OnDealDamage(m_user, MAoeZoneRef.zoneDamage, !MAoeZoneRef.isArmorAffecting,
                            null, transform, MAoeZoneRef.hasKnockback);
                    }
                    else if(MAoeZoneRef.zoneDamage < 0)
                    {
                        damageable?.OnHeal(MAoeZoneRef.zoneDamage, MAoeZoneRef.isArmorAffecting);
                    }
                    
                    if (MAoeZoneRef.isRandomKnockawayBall)
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
                        //ToDo: Status
                        //effectable?.ApplyEffect(m_zoneRef.statusBaseEffect);   
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