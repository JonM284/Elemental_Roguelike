using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.GameControllers;
using Runtime.VFX;
using UnityEngine;

namespace Runtime.Character.Creations
{
    public class RootTrapCreation: CreationBase
    {
         #region Serialized Fields
        
        [Header("Proximity EXPLOSION CREATION")]

        [SerializeField] private VFXPlayer localDiscoveryVFX;

        [SerializeField] private GameObject detonationRangeIndicator;
        
        #endregion

        #region Private Fields
        
        private float m_detonationRadius;
        
        private bool m_isHidden;

        private bool m_isPlayerSide;

        private CharacterBase m_triggeredEnemy;
        
        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }
        
        private TrapCreationData trapCreationData => creationData as TrapCreationData;
        
        #endregion

        #region Unity Events

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, m_detonationRadius);
        }

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

            var visibleLayer = LayerMask.GetMask("Default");
            creationVisuals.ForEach(g => g.layer = visibleLayer);
        }

        private CharacterBase GetEnemy()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_detonationRadius);

            if (colliders.Length == 0)
            {
                return default;
            }

            foreach (var collider in colliders)
            {
                if (!m_triggeredEnemy.IsNull())
                {
                    continue;
                }
                    
                collider.TryGetComponent(out CharacterBase character);
                if (!character)
                {
                    continue;
                }

                if (character.side == this.side)
                {
                    continue;
                }

                return character;
            }

            return default;
        }

        private bool IsInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_detonationRadius);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    if (!m_triggeredEnemy.IsNull())
                    {
                        continue;
                    }
                    
                    col.TryGetComponent(out CharacterBase character);
                    if (!character)
                    {
                        continue;
                    }

                    if (character.side == this.side)
                    {
                        continue;
                    }

                    m_triggeredEnemy = character;
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        #region CreationBase Inherited Methods

        public override void Initialize(CreationData _data, Transform _user, CharacterSide _side)
        {
            base.Initialize(_data, _user, _side);

            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = trapCreationData.GetRadius();
            
            m_isHidden = trapCreationData.GetIsHidden();

            m_triggeredEnemy = null;
            
            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = trapCreationData.GetHiddenLayer();
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

        //Create a zone
        public override void DoAction()
        {
            isDoingAction = true;

            SetCreationDiscovered();

            if (m_triggeredEnemy.IsNull())
            {
                m_triggeredEnemy = GetEnemy();
            }

            if (!m_triggeredEnemy.IsNull())
            {
                m_triggeredEnemy.characterMovement.ForceStopMovement(false);
                m_triggeredEnemy.characterMovement.SetCharacterRooted(true);
                m_triggeredEnemy.characterMovement.TeleportCharacter(transform.position);
            }
            
            hasDoneAction = true;
            isDoingAction = false;
        }

        public override void DestroyCreation()
        {
            base.DestroyCreation();
            m_triggeredEnemy.characterMovement.SetCharacterRooted(false);
            m_triggeredEnemy = null;
        }

        #endregion
    }
}