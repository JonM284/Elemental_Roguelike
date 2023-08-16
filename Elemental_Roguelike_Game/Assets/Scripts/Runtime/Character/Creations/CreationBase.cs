using System.Collections.Generic;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Selection;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Character.Creations
{
    [DisallowMultipleComponent]
    public abstract class CreationBase : MonoBehaviour, IDamageable, ISelectable
    {

        #region Serialized Fields

        [Tooltip("VFX to play when creating")]
        [SerializeField] protected VFXPlayer localSpawnVFX;

        [SerializeField] protected VFXPlayer damageVFX;
        
        [Tooltip("VFX to play if is destroyed by another player or damageable")]
        [SerializeField] protected VFXPlayer localDestroyVFX;

        [SerializeField] protected List<GameObject> creationVisuals = new List<GameObject>();
        
        #endregion

        #region Protected Fields

        protected int currentRoundCountdown;

        protected bool isDoingAction;

        protected bool hasDoneAction;

        protected int currentHP;

        protected bool isIndestructible;

        #endregion
        
        #region Accessors

        public CreationData creationData { get; private set; }

        public Transform owner { get; private set; }

        public CharacterSide side { get; private set; }
        

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += CheckTurnPass;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam -= CheckTurnPass;
        }

        #endregion
        
        //----------------------

        #region Class Implementation

        public virtual void Initialize(CreationData _data, Transform _user, CharacterSide _side)
        {
            if (_data.IsNull())
            {
                return;
            }

            if (!localSpawnVFX.IsNull())
            {
                localSpawnVFX.Play();
            }

            creationData = _data;

            owner = _user;

            side = _side;

            isDoingAction = false;

            hasDoneAction = false;

            isIndestructible = creationData.GetIsIndestructible();

            currentHP = creationData.GetHealth();

            currentRoundCountdown = creationData.GetRoundStayAmountMax();
        }

        public virtual void CheckTurnPass(CharacterSide _side)
        {
            if (_side != this.side)
            {
                return;
            }

            if (creationData.GetIsInfinite())
            {
                return;
            }

            if (currentRoundCountdown <= 0)
            {
                DestroyCreation();
            }

            currentRoundCountdown--;
        }
        
        protected void PlayDamageVFX()
        {
            if (damageVFX == null)
            {
                return;
            }

            damageVFX.PlayAt(transform.position, Quaternion.identity);
        }

        public virtual void DestroyCreation()
        {
            localDestroyVFX.PlayAt(transform.position, Quaternion.identity);
            //Cache Creation
            CreationController.Instance.ReturnToPool(this);
        }
        
        public abstract void DoMovementAction();

        public abstract void DoAction();



        #endregion
        
        //---------------------

        #region IDamageable Inherited Methods

        public void OnSelect()
        {
            
        }

        public void OnUnselected()
        {
            
        }

        //Make visuals look like normal selectable
        public void OnHover()
        {
        }

        public void OnUnHover()
        {
        }

        #endregion
        
        //-------------------------

        #region ISelectable Inherited Methods

        public void OnRevive()
        {
            
        }

        public void OnHeal(int _healAmount, bool _isHealArmor)
        {
            
        }

        public void OnDealDamage(Transform attacker, int _damageAmount, bool _armorPiercing, ElementTyping _damageElementType, Transform _knockbackAttacker,
            bool _hasKnockback)
        {
            if (isIndestructible)
            {
                return;
            }
            
            PlayDamageVFX();
            
            currentHP -= _damageAmount;

            if (currentHP <= 0)
            {
                DestroyCreation();
            }
        }

        #endregion

    }
}