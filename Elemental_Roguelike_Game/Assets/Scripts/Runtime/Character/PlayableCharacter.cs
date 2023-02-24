using Data;
using Runtime.GameControllers;
using UnityEngine;

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
            characterMovement.InitializeCharacterMovement(m_characterStatsData.baseSpeed, m_characterStatsData.movementDistance);
            characterLifeManager.InitializeCharacterHealth(m_characterStatsData.baseHealth, m_characterStatsData.baseShields,
                m_characterStatsData.currentHealth, m_characterStatsData.currentShield,m_characterStatsData.type);
            movementRangeGO.transform.localScale = Vector3.one * (m_characterStatsData.movementDistance * 2);
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

        protected override void OnWalkActionPressed()
        {
            movementRangeGO.SetActive(true);
        }

        protected override void OnBeginWalkAction()
        {
            movementRangeGO.SetActive(false);
            base.OnBeginWalkAction();
        }

        protected override void OnAttackActionPressed()
        {
            
        }

        #endregion

        #region Class Implementation

        private void OnBattleEnded()
        {
            m_characterStatsData.currentHealth = characterLifeManager.currentHealthPoints;
            m_characterStatsData.currentShield = characterLifeManager.currentShieldPoints;
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