using System.Collections.Generic;
using Runtime.Character;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.UI.DataReceivers
{
    public class BattleUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIBase window;

        [SerializeField] private GameObject playerVisuals;

        [SerializeField] private UIPopupCreator uiPopupCreator;

        [SerializeField] private GameObject playerOrderToken;

        #endregion

        #region Private Fields

        private bool isPlayerTurn;

        private List<CharacterBase> m_activeBattleOrder = new List<CharacterBase>();

        private List<CharacterBase> m_cachedBattleOrder = new List<CharacterBase>();

        #endregion

        #region Accessors

        public CharacterBase activePlayer => TurnUtils.GetActiveCharacter();

        public bool canDoAction => isPlayerTurn && !activePlayer.isBusy;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattleEnded += OnBattleEnded;
            TurnController.OnChangeCharacterTurn += OnChangeCharacterTurn;
            TurnController.OnTurnOrderChanged += OnTurnOrderChanged;
        }

        private void OnDisable()
        {
            TurnController.OnBattleEnded -= OnBattleEnded;   
            TurnController.OnChangeCharacterTurn -= OnChangeCharacterTurn;
            TurnController.OnTurnOrderChanged -= OnTurnOrderChanged;
        }

        #endregion

        #region Class Implementation

        private void OnBattleEnded()
        {
            window.Close();
        }
        
        private void OnChangeCharacterTurn(CharacterBase _character)
        {
            isPlayerTurn = _character.side == CharacterSide.PLAYER;
            playerVisuals.SetActive(isPlayerTurn);
        }

        public void OnMoveClicked()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.SetCharacterWalkAction();
        }

        public void OnAttackClicked()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterWeapon();
        }

        public void UseFirstAbility()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterAbility(0);
        }

        public void UseSecondAbility()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterAbility(1);
        }

        public void EndTurn()
        {
            if (!canDoAction)
            {
                return;
            }

            if (activePlayer.characterActionPoints > 0)
            {
                uiPopupCreator.CreatePopup();
            }
            else
            {
                ConfirmEndTurn();
            }
        }

        public void ConfirmEndTurn()
        {
            activePlayer.EndTurn();
            Debug.Log("<color=red>Player Ended Turn</color>");
        }
        
        private void OnTurnOrderChanged(List<CharacterBase> _orderList)
        {
            if (_orderList.Count == 0)
            {
                return;
            }
            
            

        }

        #endregion

    }    
}

