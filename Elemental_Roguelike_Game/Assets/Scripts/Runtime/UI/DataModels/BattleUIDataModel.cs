using System.Collections.Generic;
using Data.Sides;
using Project.Scripts.Utils;
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

        [SerializeField] private GameObject shootButton;

        [SerializeField] private UIPopupCreator uiPopupCreator;

        [SerializeField] private GameObject playerOrderToken;

        [SerializeField] private CharacterSide playerSide;

        #endregion

        #region Private Fields

        private bool isPlayerTurn;
        
        #endregion

        #region Accessors

        public CharacterBase activePlayer => TurnUtils.GetActiveCharacter();

        public bool canDoAction => isPlayerTurn && 
                                   !activePlayer.IsNull() &&!activePlayer.isBusy;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattleEnded += OnBattleEnded;
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
            TurnController.OnChangeCharacterTurn += OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
            CharacterBase.BallPickedUp += OnBallPickedUp;
        }

        private void OnDisable()
        {
            TurnController.OnBattleEnded -= OnBattleEnded;   
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
            TurnController.OnChangeCharacterTurn -= OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            CharacterBase.BallPickedUp -= OnBallPickedUp;
        }

        #endregion

        #region Class Implementation

        private void OnBattleEnded()
        {
            window.Close();
        }
        
        private void OnChangeCharacterTurn(CharacterBase _character)
        {
            isPlayerTurn = _character.side == playerSide;
            playerVisuals.SetActive(isPlayerTurn);
            if (isPlayerTurn)
            {
                shootButton.SetActive(!_character.heldBall.IsNull());
            }
        }
        
        private void OnBallPickedUp(CharacterBase _character)
        {
            if (_character == activePlayer)
            {
                shootButton.SetActive(!_character.heldBall.IsNull());
            }
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

        public void UseShootBall()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.SetCharacterThrowAction();
        }

        public void EndTurn()
        {
            if (!canDoAction)
            {
                return;
            }

            var activeTeamMembers = TurnController.Instance.GetActiveTeam();
            if (!activeTeamMembers.TrueForAll(cb => cb.characterActionPoints == 0))
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
            var activeTeamMembers = TurnController.Instance.GetActiveTeam();
            activeTeamMembers.ForEach(cb => cb.EndTurn());
            Debug.Log("<color=red>Player Ended Turn</color>");
        }
        
        private void OnChangeActiveTeam(CharacterSide characterSide)
        {
            var _isPlayerTeam = characterSide == playerSide;   
            playerVisuals.SetActive(_isPlayerTeam);
        }

        #endregion

    }    
}

