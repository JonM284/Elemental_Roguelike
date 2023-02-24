using Data;
using Project.Scripts.Data;

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
            characterMovement.InitializeCharacterMovement(m_enemyMeepleStats.baseSpeed, m_enemyMeepleStats.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_enemyMeepleStats.baseHealth, m_enemyMeepleStats.baseShields,
                m_enemyMeepleStats.currentHealth, m_enemyMeepleStats.currentShield, m_enemyMeepleStats.type);
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

        #endregion

        #region Class Implementation

        public void AssignStats(CharacterStatsData _characterData)
        {
            m_enemyMeepleStats = _characterData;
        }
        
        #endregion
        
    }
}