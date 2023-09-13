using Data;
using Data.CharacterData;
using Runtime.GameControllers;
using Runtime.Selection;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class PlayableCharacter: CharacterBase
    {

        #region Private Fields

        private CharacterStatsData m_characterStatsData;

        #endregion
        
        #region CharacterBase Inherited Methods
        
        /// <summary>
        /// Initialize all components of character
        /// Also, check for upgrade items and such, then apply here
        /// </summary>
        public override void InitializeCharacter()
        {
            var elementType = ElementUtils.GetElementTypeByGUID(m_characterStatsData.meepleElementTypeRef);
            var classType = MeepleController.Instance.GetClassByGUID(m_characterStatsData.classReferenceType);

            characterVisuals.InitializeMeepleCharacterVisuals(elementType);
            
            characterMovement.InitializeCharacterMovement(m_characterStatsData.baseSpeed, m_characterStatsData.movementDistance, m_characterStatsData.damageScore, elementType);
            
            characterLifeManager.InitializeCharacterHealth(m_characterStatsData.baseHealth, m_characterStatsData.baseShields,
                m_characterStatsData.currentHealth, m_characterStatsData.currentShield, elementType);
            
            if (m_characterStatsData.abilityReferences.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_characterStatsData.abilityReferences, elementType, classType);

                characterAnimations.InitializeAnimations(m_characterStatsData.abilityReferences, m_characterStatsData.classReferenceType ,m_characterStatsData.meepleElementTypeRef);
            }
            
            characterClassManager.InitializedCharacterPassive(classType, m_characterStatsData.agilityScore, 
                m_characterStatsData.shootingScore, m_characterStatsData.damageScore);
        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsData == null)
            {
                return 0;
            }

            return m_characterStatsData.baseSpeed;
        }

        #endregion

        #region Class Implementation

        protected override void OnBattleEnded()
        {
            m_characterStatsData.currentHealth = characterLifeManager.currentHealthPoints;
            m_characterStatsData.currentShield = characterLifeManager.currentShieldPoints;
            if (isInBattle)
            {
                InitializeCharacterBattle(false);
            }
        }

        public void AssignStats(CharacterStatsData _characterData)
        {
            m_characterStatsData = _characterData;
        }

        #endregion


    }
}