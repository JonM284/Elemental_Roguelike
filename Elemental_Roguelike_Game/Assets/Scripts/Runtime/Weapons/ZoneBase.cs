using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.VFX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Weapons
{

    public class ZoneBase: MonoBehaviour
    {
        
        #region Serialized Fields

        [SerializeField] private float m_debugRadius = 1f;

        [SerializeField] private VFXPlayer attachedVFX;
        [SerializeField] private AudioSource attachedSFX;

        [SerializeField] private GameObject aoeIndicator;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Image progressImage;

        #endregion

        #region Private Fields

        private CharacterSide m_side;

        private int m_currentTurnTimer;

        private bool hasHitTarget = false;

        private CharacterBase currentOwner;

        private int hitsAmount;
        private Collider[] hits = new Collider[20];

        private CancellationTokenSource cts = new CancellationTokenSource();

        #endregion

        #region Accessors

        public AoeZoneAbilityData AoeZoneData { get; private set; }

        private bool hasStatusEffect => !AoeZoneData.IsNull() && AoeZoneData.applicableStatusesOnHit.Any();

        #endregion
        
        #region UnityEvents

        private void OnDrawGizmos()
        {
            if(!AoeZoneData.IsNull()){
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, AoeZoneData.zoneRadius);
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
            TurnController.OnResetField += ReturnObject;
            TurnController.OnBattleEnded += ReturnObject;
            TickGameController.Tick += OnTick;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            TurnController.OnResetField -= ReturnObject;
            TurnController.OnBattleEnded -= ReturnObject;
            TickGameController.Tick -= OnTick;
        }
        

        #endregion

        public void Initialize(AoeZoneAbilityData aoeZoneData, CharacterBase owner)
        {
            if (aoeZoneData.IsNull())
            {
                Debug.Log("NO ZONE INFO");
                return;
            }

            AoeZoneData = aoeZoneData;
            
            m_currentTurnTimer = 0;

            currentOwner = owner;

            hasHitTarget = false;
            
            m_side = currentOwner.side;
            
            Debug.Log($"[Zone] Name:{aoeZoneData.abilityName} __ Type:{aoeZoneData.aoeType.ToString()} __ " +
                      $"Has Projectile?: {!aoeZoneData.displayedProjectile.IsNull()}");

            ShowIndicator(true);
        }

        private CancellationToken GetTokenSource()
        {
            if (!cts.IsNull())
            {
                cts.Cancel();
            }
                
            cts = new CancellationTokenSource();
            return cts.Token;
        }

        private void ShowIndicator(bool isActive)
        {
            aoeIndicator.SetActive(isActive);
            if(isActive && !AoeZoneData.IsNull()) aoeIndicator.transform.localScale = Vector3.one * (AoeZoneData.zoneRadius * 2f);
        }
        
        public void OnCreate()
        {
            GetTokenSource();
            
            if (AoeZoneData.aoeType != AreaOfEffectType.ON_CREATE)
            {
                PlayEffectsAsync(cts.Token).Forget();
                CheckZone();
                return;
            }
            
            OnCreateAsync(cts.Token).Forget();
        }
        
        
        private async UniTask OnCreateAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await PlayEffectsAsync(token);
            CheckZone();
            ReturnObject();
        }
        
        private void ReturnObject()
        {
            ShowIndicator(false);
            ObjectPoolController.Instance.ReturnToPool(ObjectPoolController.ZonePoolName, 
                AoeZoneData.abilityGUID, gameObject).Forget();
        }

        private async UniTask PlayEffectsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            PlayVFX(token).Forget();
            PlaySFX(token).Forget();
        }

        private async UniTask PlayVFX(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            if (attachedVFX.IsNull())
            {
                return;
            }
            
            attachedVFX.Play();
            await UniTask.WaitUntil(() => !attachedVFX.is_playing, cancellationToken: token);
        }

        private async UniTask PlaySFX(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            if (attachedSFX.IsNull())
            {
                return;
            }
            
            attachedSFX.Play();
            await UniTask.WaitUntil(() => !attachedSFX.isPlaying, cancellationToken: token);
        }

        private void OnTick()
        {
            if(AoeZoneData.aoeType != AreaOfEffectType.ALWAYS || AoeZoneData.IsNull())
                return;

            hasHitTarget = false;
            CheckZone();
            
            if (!hasHitTarget) return;
            PlayEffectsAsync(GetTokenSource()).Forget();
        }

        /// <summary>
        /// Every Turn
        /// </summary>
        private void OnChangeActiveTeam(CharacterSide side)
        {
            switch (AoeZoneData.aoeType)
            {
                case AreaOfEffectType.ON_EVERY_TURN:
                    m_currentTurnTimer++;
                    break;
                case AreaOfEffectType.ON_EVERY_ROUND:
                    if (side.IsNull() || side.sideGUID != this.m_side.sideGUID)
                        return;

                    m_currentTurnTimer++;
                    break;
                case AreaOfEffectType.ON_COUNTDOWN_END:
                    m_currentTurnTimer++;
                    if(m_currentTurnTimer < AoeZoneData.roundStayAmount)
                        return;
                    PlayEffectsAsync(GetTokenSource()).Forget();
                    CheckZone();
                    break;
                case AreaOfEffectType.ON_CREATE:
                    return;
            }
            
            
            if (m_currentTurnTimer < AoeZoneData.roundStayAmount)
            {
                PlayEffectsAsync(GetTokenSource()).Forget();
                CheckZone();
            } else
            {
                ReturnObject();
            }
        }

        private void CheckZone()
        {
            hitsAmount = Physics.OverlapSphereNonAlloc(transform.position, AoeZoneData.zoneRadius, hits,
                AoeZoneData.zoneCheckLayer);
            
            if (hitsAmount == 0)
            {
                return;
            }
            
            for(int i = 0; i < hitsAmount; i++)
            {
                hits[i].TryGetComponent(out CharacterBase _character);

                if (_character.IsNull())
                {
                    continue;
                }
                
                if (AoeZoneData.isIgnoreUser && _character == currentOwner)
                {
                    continue;
                }

                if (AoeZoneData.isStopReaction)
                {
                    _character.SetCharacterUsable(false);
                    _character.characterClassManager.SetAbleToReact(false);
                }

                if (AoeZoneData.zoneDamage > 0)
                {
                    _character.OnDealDamage(this.transform, AoeZoneData.zoneDamage, !AoeZoneData.isArmorAffecting,
                        null, transform, AoeZoneData.hasKnockback);
                }
                else if(AoeZoneData.zoneDamage < 0)
                {
                    _character.OnHeal(AoeZoneData.zoneDamage, AoeZoneData.isArmorAffecting);
                }
                    
                if (AoeZoneData.isRandomKnockawayBall)
                {
                   _character.characterBallManager.KnockBallAway();
                }

                if (hasStatusEffect)
                {
                    AoeZoneData.applicableStatusesOnHit.ForEach(s => _character.ApplyStatus(s).Forget());   
                }

                hasHitTarget = true;
            }
        }
    }
}