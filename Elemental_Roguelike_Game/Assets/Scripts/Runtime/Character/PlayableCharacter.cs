using Data;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class PlayableCharacter: CharacterBase
    {

        #region Serialized Fields

        [SerializeField] private GameObject movementRangeGO;

        [SerializeField] private GameObject attackRangeGO;

        #endregion

        #region Private Fields

        private CharacterStatsData m_characterStatsData;

        #endregion
        
        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeCharacterTurn += OnChangeCharacterTurn;
            TurnController.OnBattleEnded += OnBattleEnded;
        }

        private void OnDisable()
        {
            TurnController.OnChangeCharacterTurn -= OnChangeCharacterTurn;
            TurnController.OnBattleEnded -= OnBattleEnded;
        }

        #endregion


        #region CharacterBase Inherited Methods
        
        public override void InitializeCharacter()
        {
            var elementType = ElementUtils.GetElementTypeByGUID(m_characterStatsData.meepleElementTypeRef);
            characterVisuals.InitializeMeepleCharacterVisuals(elementType);
            characterMovement.InitializeCharacterMovement(m_characterStatsData.baseSpeed, m_characterStatsData.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_characterStatsData.baseHealth, m_characterStatsData.baseShields,
                m_characterStatsData.currentHealth, m_characterStatsData.currentShield, elementType);
            movementRangeGO.transform.localScale = Vector3.one * (m_characterStatsData.movementDistance * 2);
            if (m_characterStatsData.abilityReferences.Count > 0)
            {
                characterAbilityManager.InitializeCharacterAbilityList(m_characterStatsData.abilityReferences);
            }

            var weaponData = WeaponUtils.GetDataByRef(m_characterStatsData.weaponReference);
            var weaponElementType = ElementUtils.GetElementTypeByGUID(m_characterStatsData.weaponElementTypeRef);
            characterWeaponManager.InitializeCharacterWeapon(weaponData, weaponElementType);
        }

        public override int GetInitiativeNumber()
        {
            if (m_characterStatsData == null)
            {
                return 0;
            }

            return m_characterStatsData.initiativeNumber;
        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsData == null)
            {
                return 0;
            }

            return m_characterStatsData.baseSpeed;
        }

        protected override void CharacterDeath()
        {
            CharacterUtils.DeletePlayerMeeple(m_characterStatsData.id);
            CharacterUtils.CachePlayerMeeple(this);
        }

        protected override void OnWalkActionPressed()
        {
            
        }

        #endregion

        #region Class Implementation

        private void OnBattleEnded()
        {
            m_characterStatsData.currentHealth = characterLifeManager.currentHealthPoints;
            m_characterStatsData.currentShield = characterLifeManager.currentShieldPoints;
            if (isInBattle)
            {
                InitializeCharacterBattle(false);
            }
        }
        
        private void OnChangeCharacterTurn(CharacterBase _characterBase)
        {
            if (_characterBase != this)
            {
                return;
            }

            if (!isAlive)
            {
                return;
            }
            
            Debug.Log("Player start");
            ResetCharacterActions();
        }

        public void AssignStats(CharacterStatsData _characterData)
        {
            m_characterStatsData = _characterData;
        }

        #endregion

        
    }
}