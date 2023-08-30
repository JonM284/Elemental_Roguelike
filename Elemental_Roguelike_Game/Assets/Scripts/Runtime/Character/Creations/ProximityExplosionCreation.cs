using System;
using System.Collections;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Status;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Character.Creations
{
    public class ProximityExplosionCreation: CreationBase
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
        
        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }

        public ProximityCreationData proximityCreationData { get; private set; }

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

        private bool IsInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_detonationRadius);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    col.TryGetComponent(out CharacterBase character);
                    if (!character)
                    {
                        continue;
                    }

                    if (character.side == this.side)
                    {
                        continue;
                    }
                    
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
            proximityCreationData = _data as ProximityCreationData;

            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = proximityCreationData.GetRadius();
            
            m_isHidden = proximityCreationData.GetIsHidden();
            
            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = proximityCreationData.GetHiddenLayer();
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
            SetCreationDiscovered();
            
            isDoingAction = true;
            proximityCreationData.GetZoneInfo().PlayAt(transform.position, owner);
            hasDoneAction = true;
            isDoingAction = false;
            DestroyCreation();
        }

        #endregion
        
    }
}