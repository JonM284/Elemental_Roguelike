using System;
using Project.Scripts.Utils;
using UnityEngine;
using Rewired;
using Runtime.GameControllers;

namespace Runtime.Gameplay.Inputs
{
    public class GameplayInputsManager: MonoBehaviour
    {

        #region Private Fields

        private int playerID = 0;

        private Player m_player;

        private ControllerStatusChangedEventArgs m_currentEventArgs;

        #endregion

        #region Accessor

        public bool isControllerConnected => !m_currentEventArgs.IsNull() &&
                                             m_currentEventArgs.controllerType == ControllerType.Joystick;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            ReInput.ControllerConnectedEvent += ReInputOnControllerStatusChangeEvent;
            ReInput.ControllerDisconnectedEvent += ReInputOnControllerStatusChangeEvent;
        }

        private void OnDisable()
        {
            ReInput.ControllerConnectedEvent -= ReInputOnControllerStatusChangeEvent;
            ReInput.ControllerDisconnectedEvent += ReInputOnControllerStatusChangeEvent;
        }

        private void Start()
        {
            m_player = ReInput.players.GetPlayer(playerID);
        }

        private void Update()
        {
            if (m_player.GetButtonDown("NextCharacter"))
            {
                TurnController.Instance.SelectAvailablePlayer(true);
            }else if (m_player.GetButtonDown("PrevCharacter"))
            {
                TurnController.Instance.SelectAvailablePlayer(false);
            }else if (m_player.GetButtonDown("NextAction"))
            {
                
            }else if (m_player.GetButtonDown("PrevAction"))
            {
                
            }
        }

        #endregion
        
        #region Class Implementation

        private void ReInputOnControllerStatusChangeEvent(ControllerStatusChangedEventArgs _event)
        {
            
        }

        #endregion
        
    }
}