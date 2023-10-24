using System.Collections;
using System.Collections.Generic;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.VFX;
using UnityEngine;

namespace Runtime.Character.Creations
{
    public class ProximityTurretMeleeCreation: CreationBase
    {
        #region Read-Only

        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        #endregion
        
        #region Serialized Fields
        
        [Header("Proximity EXPLOSION CREATION")]

        [SerializeField] private VFXPlayer localDiscoveryVFX;

        [SerializeField] private GameObject detonationRangeIndicator;

        [SerializeField] private Transform meleePosition;

        [SerializeField] private float meleeRadius = 0.15f;
        
        #endregion

        #region Private Fields
        
        private float m_detonationRadius;
        
        private bool m_isHidden;

        private bool m_isPlayerSide;

        private bool m_dealsKnockback;

        private int m_damageAmount;

        private Animator m_animator;

        private ElementTyping elementTyping;

        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }

        public MeleeTurretCreationData meleeTurretCreationData { get; private set; }

        public Animator animator => CommonUtils.GetRequiredComponent(ref m_animator, () =>
        {
            var a = GetComponent<Animator>();
            return a;
        });

        #endregion
        
        #region Unity Events

        private void Update()
        {
            if (isDoingAction || hasDoneAction)
            {
                return;
            }
            
            if (IsInRange())
            {
                Debug.Log("Should Do Action");
                DoAction();
            }
        }

        #endregion
        
        #region Class Implementation

            private void SetCreationDiscovered()
            {
                isDiscovered = true;
                
                if (!localDiscoveryVFX.IsNull())
                {
                    localDiscoveryVFX.Play();
                }
                
                this.gameObject.layer = LayerMask.GetMask("Default");
            }

            private bool IsInRange()
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position,  m_detonationRadius);

                if (colliders.Length > 0)
                {
                    foreach (var col in colliders)
                    {
                        col.TryGetComponent(out CharacterBase _character);

                        if (!_character.IsNull())
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }

            //ToDo: Could just do damage to everyone around
            private IEnumerator C_FireAtSurroundingEnemies(List<CharacterBase> _enemies)
            {
                if (_enemies.Count == 0)
                {
                    yield break;
                }

                foreach (var currentEnemy in _enemies)
                {
                    var targetPosition = currentEnemy.transform.position;
                        
                    var directionToTarget = targetPosition - transform.position;
                        
                    transform.forward = directionToTarget;

                    meleePosition.position = targetPosition;
                    
                    StartMeleeAnimation();
                    if (!animator.IsNull())
                    {
                        yield return new WaitUntil(() => !animator.GetBool(IsAttacking));
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                }

            }

            public List<CharacterBase> GetSurroundingEnemies()
            {
                List<CharacterBase> foundEnemies = new List<CharacterBase>();

                Collider[] colliders = Physics.OverlapSphere(transform.position,  m_detonationRadius);

                if (colliders.Length > 0)
                {
                    foreach (var col in colliders)
                    {
                        col.TryGetComponent(out CharacterBase _character);
                        //Not character
                        if (_character.IsNull())
                        {
                            continue;
                        }

                        //Character is ally
                        if (_character.side == this.side)
                        {
                            continue;
                        }
                        
                        foundEnemies.Add(_character);
                    }
                }
                
                return foundEnemies;
            }

        #endregion
        
        #region CreationBase Inherited Methods

        public override void Initialize(CreationData _data, Transform _user, CharacterSide _side)
        {
            base.Initialize(_data, _user, _side);
            meleeTurretCreationData = _data as MeleeTurretCreationData;

            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = meleeTurretCreationData.GetRadius();
            
            m_isHidden = meleeTurretCreationData.GetIsHidden();

            m_damageAmount = meleeTurretCreationData.GetDamageAmount();

            elementTyping = meleeTurretCreationData.GetElement();

            m_dealsKnockback = meleeTurretCreationData.GetDoesKnockback();
            
            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = meleeTurretCreationData.GetHiddenLayer();
                    creationVisuals.ForEach(g => g.layer = hiddenLayer);
                }
            }
            
            //* 2 because it's a radius
            detonationRangeIndicator.transform.localScale = Vector3.one * (m_detonationRadius * 2);
            
            if (!localSpawnVFX.IsNull())
            {
                localSpawnVFX.Play();
            }
            
        }
        
        public override void DoMovementAction()
        {
            //None
        }

        public override void DoAction()
        {
            isDoingAction = true;

            SetCreationDiscovered();

            var _surroundingEnemies = GetSurroundingEnemies();

            
            
            if (_surroundingEnemies.Count > 0)
            {
                AttackAround();
            }
            
            hasDoneAction = true;
            isDoingAction = false;
        }

        public override void CheckTurnPass(CharacterSide _side)
        {
            base.CheckTurnPass(_side);
            hasDoneAction = false;
            isDoingAction = false;
        }

        public void AttackAround()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position,  m_detonationRadius);

            if (colliders.Length == 0)
            {
                return;
            }

            foreach (var collider in colliders)
            {
                if(collider.transform == this.transform)
                {
                    continue;
                }
                
                collider.TryGetComponent(out IDamageable damageable);
                damageable?.OnDealDamage(owner, m_damageAmount, false, elementTyping, this.transform, m_dealsKnockback);
            }
        }

        //Animation Event
        public void OnAttackEnded()
        {
            if (animator.IsNull())
            {
                return;
            }
            
            animator.SetBool(IsAttacking, false);
        }

        //Animation Event
        public void Attack()
        {
            Collider[] colliders = Physics.OverlapSphere(meleePosition.position,  meleeRadius);

            if (colliders.Length == 0)
            {
                return;
            }

            foreach (var collider in colliders)
            {
                collider.TryGetComponent(out IDamageable damageable);
                damageable?.OnDealDamage(owner, m_damageAmount, false, elementTyping, this.transform, m_dealsKnockback);
            }
        }
        
        
        private void StartMeleeAnimation()
        {
            if (animator.IsNull())
            {
                return;
            }
            
            animator.SetBool(IsAttacking, true);
        }

        #endregion
        
        
    }
}