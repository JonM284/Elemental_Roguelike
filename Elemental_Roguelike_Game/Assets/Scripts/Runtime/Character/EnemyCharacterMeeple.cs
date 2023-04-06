using Data;
using Project.Scripts.Data;
using Utils;

namespace Runtime.Character
{
    public class EnemyCharacterMeeple: CharacterBase
    {
        
        #region Private Fields

        private CharacterStatsData m_enemyMeepleStats;

        #endregion

        #region CharacterBase Inherited Methods

        public override void InitializeCharacter()
        {
            var elementType = ElementUtils.GetElementTypeByGUID(m_enemyMeepleStats.meepleElementTypeRef);
            characterVisuals.InitializeMeepleCharacterVisuals(elementType);
            characterMovement.InitializeCharacterMovement(m_enemyMeepleStats.baseSpeed, m_enemyMeepleStats.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_enemyMeepleStats.baseHealth, m_enemyMeepleStats.baseShields,
                m_enemyMeepleStats.currentHealth, m_enemyMeepleStats.currentShield, elementType);
            if (m_enemyMeepleStats.abilityReferences.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_enemyMeepleStats.abilityReferences);
            }
            
            var weaponData = WeaponUtils.GetDataByRef(m_enemyMeepleStats.weaponReference);
            var weaponElementType = ElementUtils.GetElementTypeByGUID(m_enemyMeepleStats.weaponElementTypeRef);
            characterWeaponManager.InitializeCharacterWeapon(weaponData, weaponElementType);
        }

        public override int GetInitiativeNumber()
        {
            if (m_enemyMeepleStats == null)
            {
                return 0;
            }

            return m_enemyMeepleStats.initiativeNumber;
        }

        public override float GetBaseSpeed()
        {
            if (m_enemyMeepleStats == null)
            {
                return 0;
            }

            return m_enemyMeepleStats.baseSpeed;
        }

        protected override void CharacterDeath()
        {
            this.CacheEnemy();
        }

        #endregion

        #region Class Implementation

        public void AssignStats(CharacterStatsData _characterData)
        {
            m_enemyMeepleStats = _characterData;
        }
        
        #endregion
        
    }
}