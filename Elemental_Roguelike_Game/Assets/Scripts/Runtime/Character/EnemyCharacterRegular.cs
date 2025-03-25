using Data.CharacterData;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Character.AI;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class EnemyCharacterRegular: CharacterBase
    {

        #region CharacterBase Inherited Methods

        /// <summary>
        /// Initialize all components of character
        /// Also, check for upgrade items and such, then apply here
        /// </summary>
        public override void InitializeCharacter(CharacterStatsBase _characterStats)
        {
            side = ScriptableDataController.Instance.GetSideByGuid(characterSideRef);

            m_characterStatsBase = _characterStats;

            InitializeCharacterMarker();
            
            if (characterVisuals.isMeeple)
            {
                characterVisuals.InitializeMeepleCharacterVisuals(m_characterStatsBase.typing);
            }
            else
            {
                characterVisuals.InitializeCharacterVisuals();
            }
            
            var m_moveDistance = (m_characterStatsBase.agilityScore/100f) * maxMovementDistance;
            
            characterMovement.InitializeCharacterMovement(m_characterStatsBase.baseSpeed, m_moveDistance, m_characterStatsBase.tackleDamageAmount, m_characterStatsBase.typing, isGoalie);
            characterLifeManager.InitializeCharacterHealth(m_characterStatsBase.baseHealth, m_characterStatsBase.baseShields, m_characterStatsBase.baseHealth,
                m_characterStatsBase.baseShields, m_characterStatsBase.typing);
            
            if (m_characterStatsBase.abilities.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_characterStatsBase.abilities);
            }
            
            characterClassManager.InitializedCharacterPassive(m_characterStatsBase.classTyping, m_characterStatsBase.agilityScore,
            m_characterStatsBase.shootingScore, m_characterStatsBase.passingScore ,m_characterStatsBase.tackleScore);

            TryGetComponent(out EnemyAIBase _enemyAI);

            if (!_enemyAI.IsNull())
            {
                _enemyAI.SetupBehaviorTrees();
            }
            
        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsBase == null)
            {
                return 0;
            }

            return m_characterStatsBase.baseSpeed;
        }


        protected override void OnBattleEnded()
        {
            //Undecided
        }

        #endregion
    }
}