﻿using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class EnemyCharacterRegular: CharacterBase
    {

        #region CharacterBase Inherited Methods

        public override void InitializeCharacter()
        {
            characterVisuals.InitializeCharacterVisuals(); 
            characterMovement.InitializeCharacterMovement(m_characterStatsBase.baseSpeed, m_characterStatsBase.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_characterStatsBase.baseHealth, m_characterStatsBase.baseShields, m_characterStatsBase.baseHealth,
                m_characterStatsBase.baseShields, m_characterStatsBase.typing);
            if (m_characterStatsBase.abilities.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_characterStatsBase.abilities);
            }

            characterWeaponManager.InitializeCharacterWeapon(m_characterStatsBase.weaponData, m_characterStatsBase.weaponTyping);
        }

        public override int GetInitiativeNumber()
        {
            if (m_characterStatsBase == null)
            {
                return 0;
            }

            return m_characterStatsBase.initiativeNumber;
        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsBase == null)
            {
                return 0;
            }

            return m_characterStatsBase.baseSpeed;
        }

        protected override void CharacterDeath()
        {
            this.CacheEnemy();
        }

        #endregion
    }
}