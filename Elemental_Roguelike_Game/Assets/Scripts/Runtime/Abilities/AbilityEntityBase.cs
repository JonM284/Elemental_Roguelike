using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Abilities
{
    public abstract class AbilityEntityBase: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] protected GameObject abilityCastRangeIndicator, 
            abilityPositionIndicator;
        [SerializeField] protected LineRenderer abilityDirectionIndicator;
        
        [SerializeField] protected LayerMask wallLayer;
        
        [SerializeField] protected List<MeshRenderer> changableMR = new List<MeshRenderer>();

        #endregion
        
        #region Protected Fields

        protected List<Collider> m_previouslyHitColliders = new List<Collider>();

        protected List<string> m_categoryGUIDs = new List<string>();
        
        protected float speedAmountMax;
        protected float rangeAmountMax;
        protected float lifeTimeMax;

        protected Vector3 endPosition, targetPosition;
        protected Transform targetTransform;

        protected bool lastActiveState;

        protected AudioSource audioSourceRef;
        
        protected RaycastHit[] hitWalls = new RaycastHit[3];
        protected int hitWallsAmount;

        protected CancellationTokenSource cts = new CancellationTokenSource();
        
        #endregion

        #region Private Fields

        private float cooldownModifier = 1f;
        //hit = damage and Knockback modifier
        private float hitModifier = 1f;
        private float lifeTimeModifier = 1f;
        private float speedModifier = 1f;
        private float rangeModifier = 1f;
        private float knockbackAmountMax;
        private float damageAmountMax;
        private float markerYOffset = 0.05f;

        #endregion
        
        #region IAbility Inherited Methods

        public bool canUseAbility => abilityCooldownCurrent <= 0;
        public float abilityCooldownCurrent { get; protected set; }
        public float abilityCooldownMax { get; set; }

        public Vector3 aimDirection { get; set; }
        public CharacterBase currentOwner { get; set; }


        #region Accessors

        public AbilityData abilityData { get; private set; }
        
        //Cooldown, Knockback, Damage, Scale, lifeTime, speed, range
        public float currentKnockback => knockbackAmountMax * hitModifier;
        public float currentDamage => damageAmountMax * hitModifier;
        public float currentLifetime => lifeTimeMax * lifeTimeModifier;
        public float currentSpeed => speedAmountMax * speedModifier;
        public float currentRange => rangeAmountMax * rangeModifier;

        public float cooldownReductionModifier => cooldownModifier;

        public float currentScale { get; set; }

        public AudioSource aSource => CommonUtils.GetRequiredComponent(ref audioSourceRef,  GetComponent<AudioSource>);

        #endregion
        
        /// <summary>
        /// Initialize Ability WITH INJECTION -> reduces amount of necessary prefabs
        /// </summary>
        /// <param name="_owner">Owner Player</param>
        /// <param name="_data">Actual Data</param>
        public virtual async UniTask InitializeAbility(CharacterBase owner, AbilityData data, bool _canUseOnStart = true)
        {
            if (owner.IsNull() || data.IsNull())
            {
                return;
            }
            
            abilityData = data;
            currentOwner = owner;

            #region ------- Changable Ability Stats --------

            abilityCooldownMax = abilityData.abilityTurnCooldownTimer;
            abilityCooldownCurrent = 0;
            damageAmountMax = abilityData.abilityDamageAmount;
            knockbackAmountMax = abilityData.abilityKnockbackAmount;
            rangeAmountMax = abilityData.abilityRange;
            currentScale = abilityData.abilityScale;

            #endregion
            
            if (_canUseOnStart)
            {
                ResetAbilityForUse();
            }
            else
            {
                SetAbilityUnusable();
            }
            
            SetCategoryGUIDs();
            await PreLoadNecessaryObjects();
            
            Debug.Log($"[Ability][Step] {abilityData.abilityName} was Initialized", gameObject);
        }
        
        protected CancellationToken GetTokenSource()
        {
            if (!cts.IsNull())
            {
                cts.Cancel();
            }
                
            cts = new CancellationTokenSource();
            return cts.Token;
        }

        /// <summary>
        /// Not actually sure. ToDo: look up in dodgeball game!
        /// </summary>
        public void SetCategoryGUIDs()
        {
            if (abilityData.abilityCategories.Count == 0)
            {
                return;
            }

            foreach (var _category in abilityData.abilityCategories)
            {
                m_categoryGUIDs.Add(_category.abilityCategoryGUID);
            }
        }

        //ToDo: change to override instead of virtual
        /// <summary>
        /// Preload all necessary status effects, projectiles, zones, creations, vfx, etc.
        /// </summary>
        public virtual async UniTask PreLoadNecessaryObjects(){}

        /// <summary>
        /// Show / Hide attack range
        /// </summary>
        /// <param name="_isActive"></param>
        public virtual void ShowAttackIndicator(bool _isActive)
        {
            abilityCastRangeIndicator.SetActive(_isActive);
            if(_isActive) abilityCastRangeIndicator.transform.localScale = Vector3.one * (currentRange * 2f);
            var isPositionHighlight = abilityData.targetType is AbilityTargetType.FREE or AbilityTargetType.LOCATION;
            abilityPositionIndicator.SetActive(_isActive && isPositionHighlight);
            var isDirectionHighlight = abilityData.targetType == AbilityTargetType.DIRECTIONAL;
            abilityDirectionIndicator.gameObject.SetActive(_isActive && isDirectionHighlight);
        }

        /// <summary>
        /// Is this ability an ability where it creates something that has a range?
        /// AKA, type creation or aoe
        /// </summary>
        /// <returns></returns>
        protected bool IsEffectAreaAbility()
        {
            return abilityData is AoeZoneAbilityData or CreationAbilityData;
        }
        
        public void MarkHighlight(Vector3 position)
        {
            switch (abilityData.targetType)
            {
                case AbilityTargetType.LOCATION: case AbilityTargetType.FREE:
                    var direction = position - transform.position;
                    if (direction.magnitude > abilityData.abilityRange)
                    {
                        return;
                    }
                    abilityPositionIndicator.transform.position = new Vector3(position.x, position.y + markerYOffset, position.z);
                    break;
                case AbilityTargetType.DIRECTIONAL:
                    var localDirection = transform.InverseTransformDirection(position - transform.position);
                    var finalPoint = (localDirection.normalized * abilityData.abilityRange);
                    abilityDirectionIndicator.SetPosition(1, new Vector3(finalPoint.x, finalPoint.z, 0));
                    break;
            }
        }

        /// <summary>
        /// Confirm Ability Location (position)
        /// </summary>
        /// <param name="_inputPosition"></param>
        public virtual void SelectPosition(Vector3 userInputPosition)
        {
            if (userInputPosition.IsNan())
            {
                return;
            }

            targetPosition = userInputPosition;
            
            ShowAttackIndicator(false);
        }


        /// <summary>
        /// Confirm Ability Location (target)
        /// </summary>
        /// <param name="_inputTransform"></param>
        public virtual void SelectTarget(Transform userSelectedTransform)
        {
            if (userSelectedTransform.IsNull())
            {
                return;
            }

            targetTransform = userSelectedTransform;
            
            ShowAttackIndicator(false);
        }

        /// <summary>
        /// Called from other scripts to use abilitu
        /// </summary>
        public async UniTask UseAbility()
        {
            var hasAbilityTarget = !targetTransform.IsNull() || !targetPosition.IsNan();
            if (currentOwner.IsNull() || !hasAbilityTarget)
            {
                return;
            }
            
            await PerformAbilityAction();

            OnAbilityUsed();
        }
        
        /// <summary>
        /// Perform actual ability actions
        /// </summary>
        protected virtual async UniTask PerformAbilityAction(){}

        /// <summary>
        /// Called after ability use
        /// </summary>
        public virtual void OnAbilityUsed()
        {
            SetAbilityUnusable();
        }

        
        /// <summary>
        /// Set ability to unusable
        /// </summary>
        protected virtual void SetAbilityUnusable()
        {
            abilityCooldownCurrent = abilityCooldownMax;
        }
        
        /// <summary>
        /// Set ability to usable
        /// </summary>
        public virtual void ResetAbilityForUse()
        {
            //Reset Variables
            abilityCooldownCurrent = 0;
            m_previouslyHitColliders.Clear();
        }

        public void UpdateAbilityCooldown()
        {
            abilityCooldownCurrent--;
        }
        

        //Example: object[] arg = {Cooldown, Hit [damage and Knockback], Scale};
        //Projectile -> MaxLifetime, Speed
        //Dash -> Offset
        /// <summary>
        /// Used to upgrade abilities during the match.
        /// [Note] Currently unused ToDo: determine use 
        /// </summary>
        /// <param name="_arguments"></param>
        public virtual void UpdateAbilityStats(params object[] _arguments)
        {
            cooldownModifier += (float)_arguments[0];
            hitModifier += (float)_arguments[1];
            currentScale += (float)_arguments[2];
            lifeTimeModifier += (float)_arguments[3];
            speedModifier += (float)_arguments[4];
            rangeModifier += (float)_arguments[5];
        }

        /// <summary>
        /// Flag for whether or not the ability is of a specified category
        /// </summary>
        /// <param name="_checkCategory"></param>
        /// <returns></returns>
        public bool ContainsCategory(AbilityCategories _checkCategory)
        {
            return m_categoryGUIDs.Count != 0 && m_categoryGUIDs.Contains(_checkCategory.abilityCategoryGUID);
        }

        /// <summary>
        /// Play a random SFX assigned to the ability
        /// </summary>
        protected void PlayRandomSound()
        {
            if (abilityData.abilityUseSFX.Count == 0)
            {
                return;
            }

            aSource.pitch = Random.Range(0.9f, 1.1f);
            aSource.PlayOneShot(abilityData.abilityUseSFX[Random.Range(0, abilityData.abilityUseSFX.Count)]);
        }
        
        
        /// <summary>
        /// Used to determine the end point, if the user is aiming towards a wall.
        /// ie: teleport to wall instead of outside map if aiming at a wall
        /// </summary>
        protected virtual void GetEndPosition()
        {
            hitWallsAmount = Physics.RaycastNonAlloc(currentOwner.transform.position, aimDirection, 
                hitWalls, currentRange, wallLayer);
            
            endPosition = hitWallsAmount > 0 ? hitWalls[0].point : 
                currentOwner.transform.position + (aimDirection.normalized * currentRange);
        }

        #endregion
        
        

    }
}