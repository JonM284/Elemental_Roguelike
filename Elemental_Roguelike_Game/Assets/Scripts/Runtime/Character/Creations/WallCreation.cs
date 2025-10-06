using System;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations.CreationDatas;
using Runtime.Damage;
using Runtime.Gameplay;
using Runtime.Status;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Character.Creations
{
    [RequireComponent(typeof(BoxCollider))]
    public class WallCreation: CreationBase
    {

        #region Private Fields

        private WallCreationData wallCreationData;

        private int m_damage;

        private Status.StatusEntityBase _mApplicableStatusEntityBase;

        private ElementTyping m_elementTyping;

        private bool m_isArmorPiercing;

        private int m_amountDecreaseShot;

        private bool m_isStopProjectiles;

        #endregion

        #region Unity Events

        private void OnTriggerEnter(Collider other)
        {
            //Is it the ball?
            other.TryGetComponent(out BallBehavior ballBehavior);

            if (ballBehavior)
            {
                ballBehavior.ReduceForce(m_amountDecreaseShot);
                return;
            }

            other.TryGetComponent(out ProjectileBase projectileBase);
            if (projectileBase)
            {
                if (m_isStopProjectiles)
                {
                    projectileBase.ForceStopProjectile();
                }
                return;
            }

            //If it isn't the ball, check if it can be damaged or status applied
            other.TryGetComponent(out IEffectable effectable);
            other.TryGetComponent(out IDamageable damageable);
            if (!effectable.IsNull())
            {
                if (!_mApplicableStatusEntityBase.IsNull())
                {
                    //ToDo:Status
                    //effectable.ApplyEffect(_mApplicableStatusBase);
                }
            }

            if (!damageable.IsNull())
            {
                damageable.OnDealDamage(owner, m_damage, m_isArmorPiercing, m_elementTyping, transform, false);
            }
            
        }

        #endregion

        #region CreationBase Inherited Methods

        public override void Initialize(CreationData _data, Transform _user, CharacterSide _side)
        {
            base.Initialize(_data, _user, _side);
            wallCreationData = _data as WallCreationData;

            m_damage = wallCreationData.GetDamage();

            _mApplicableStatusEntityBase = wallCreationData.GetStatus();

            m_elementTyping = wallCreationData.GetElementType();

            m_isArmorPiercing = wallCreationData.IsArmorPiercing();

            m_amountDecreaseShot = wallCreationData.GetShotDecreaseAmount();

            m_isStopProjectiles = wallCreationData.IsStopProjectiles();
        }

        public override void DoMovementAction()
        {
            //none
        }

        public override void DoAction()
        {
            //none - Check OnTriggerEnter
        }
        
        #endregion
        
        
    }
}