using System.Collections;
using System.Collections.Generic;
using Data;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.GameControllers;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Character.Creations
{
    public class ProximityTurretCreation: CreationBase
    {
        
        
        #region Serialized Fields
        
        [Header("Proximity TURRET CREATION")]

        [SerializeField] private VFXPlayer localDiscoveryVFX;

        [SerializeField] private GameObject detonationRangeIndicator;
        
        [SerializeField] private Transform projectileFirePos;
        
        #endregion

        #region Private Fields
        
        private float m_detonationRadius;
        
        private bool m_isHidden;

        private bool m_isPlayerSide;

        private int m_numOfShots;

        private ProjectileInfo m_projectileInfo;
        
        #endregion
        
        #region Accessors

        public bool isDiscovered { get; private set; }

        public TurretCreationData turretCreationData { get; private set; }

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

        private IEnumerator C_FireAtSurroundingEnemies(List<CharacterBase> _enemies)
        {
            if (_enemies.Count == 0)
            {
                yield break;
            }

            foreach (var currentEnemy in _enemies)
            {
                for (int i = 0; i < m_numOfShots; i++)
                {
                    var targetPosition = currentEnemy.transform.position;
                    
                    var directionToTarget = targetPosition - transform.position;
                    
                    transform.forward = directionToTarget;
                    
                    /*m_projectileInfo.PlayAt(owner ,projectileFirePos.position, transform.forward,
                        targetPosition);*/
                    
                    yield return new WaitForSeconds(0.5f);
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
            turretCreationData = _data as TurretCreationData;
            
            isDoingAction = false;
            hasDoneAction = false;

            m_detonationRadius = turretCreationData.GetRadius();
            
            m_isHidden = turretCreationData.GetIsHidden();
            
            //If is hidden proximity creation, set to hidden layer. But only do this if it's not the player's creation
            if (m_isHidden)
            {
                isDiscovered = false;
                m_isPlayerSide = _side == TurnController.Instance.playersSide;
                if (!m_isPlayerSide)
                {
                    var hiddenLayer = turretCreationData.GetHiddenLayer();
                    creationVisuals.ForEach(g => g.layer = hiddenLayer);
                }
            }

            m_numOfShots = turretCreationData.GetNumOfShots();

            m_projectileInfo = turretCreationData.GetProjectile();
            
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

        //Fire projectiles at all surrounding enemies
        public override void DoAction()
        {
            isDoingAction = true;

            SetCreationDiscovered();

            var _surroundingEnemies = GetSurroundingEnemies();

            if (_surroundingEnemies.Count > 0)
            {
                StartCoroutine(C_FireAtSurroundingEnemies(_surroundingEnemies));
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

        #endregion
        
        
        
    }
}