using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class EnemyCharacterRegular: CharacterBase
    {

        #region Serialized Fields

        [SerializeField] private Transform characterVisualParent;

        #endregion

        #region Private Fields

        private CharacterStatsBase m_enemyStats;

        #endregion

        #region CharacterBase Inherited Methods

        public override void InitializeCharacter()
        {
            if (characterVisuals.characterModel == null)
            {
                var _visual = m_enemyStats.characterVisualReference.CloneAddressableReturn(characterVisualParent);
                characterVisuals.InitializeCharacterVisuals(_visual);    
            }
            characterMovement.InitializeCharacterMovement(m_enemyStats.baseSpeed, m_enemyStats.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_enemyStats.baseHealth, m_enemyStats.baseShields, m_enemyStats.baseHealth,
                m_enemyStats.baseShields, m_enemyStats.typing);
            if (m_enemyStats.abilities.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_enemyStats.abilities);
            }

            characterWeaponManager.InitializeCharacterWeapon(m_enemyStats.weaponData, m_enemyStats.weaponTyping);
        }

        public override int GetInitiativeNumber()
        {
            if (m_enemyStats == null)
            {
                return 0;
            }

            return m_enemyStats.initiativeNumber;
        }

        public override float GetBaseSpeed()
        {
            if (m_enemyStats == null)
            {
                return 0;
            }

            return m_enemyStats.baseSpeed;
        }

        protected override void CharacterDeath()
        {
            this.CacheEnemy();
        }

        #endregion

        #region Class Implementation

        public void AssignStats(CharacterStatsBase _characterData)
        {
            m_enemyStats = _characterData;
        }

        #endregion
    }
}