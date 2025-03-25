using System;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Steamworks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI.DataModels
{
    public class MainMenuDataModel: MonoBehaviour
    {

        #region Singleton

        public static MainMenuDataModel Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private UIWindowData tournamentUIWindowData;

        [SerializeField] private UIWindowData settingsUIWindowData;

        [SerializeField] private SceneName roguelikeSceneName;

        [SerializeField] private GameObject m_mainMenu, m_onlineMenu, m_lobbyMenu;

        [SerializeField] private TMP_InputField m_joinLobbyField;

        [SerializeField] private TMP_Text m_lobbyName, m_lobbyID;

        [SerializeField] private GameObject m_startLobbyButton;

        [Header("Game Lobby")] 
        [SerializeField] private TMP_Text m_joinedPlayerName;
        [SerializeField] private RawImage m_avatarIcon;

        #endregion

        #region Unity Events

        private void Awake()
        {
            if (!Instance.IsNull())
            {
                Destroy(this.gameObject);
                return;
            }
            
            Instance = this;
        }

        #endregion

        #region Class Implementation

        public void TournamentPressed()
        {
            if (tournamentUIWindowData.IsNull())
            {
                return;
            }
            
            UIUtils.OpenUI(tournamentUIWindowData);
        }

        public void VersusPressed()
        {
            //Show options -> Create or Join Lobby -> Change screens
            
            OpenOnlineMenu();
        }

        public void OpenMainMenu()
        {
            CloseAllMenus();
            m_mainMenu.SetActive(true);
        }

        public void OpenOnlineMenu()
        {
            CloseAllMenus();
            m_onlineMenu.SetActive(true);
        }

        private void CloseAllMenus()
        {
            m_mainMenu.SetActive(false);
            m_onlineMenu.SetActive(false);
            m_lobbyMenu.SetActive(false);
        }
        
        public void OpenLobby()
        {
            CloseAllMenus();
            m_lobbyMenu.SetActive(true);
        }

        public void CreateLobby()
        {
            OnlineGameController.Instance.CreateLobby();
        }

        public void JoinLobby()
        {
            if (m_joinLobbyField.IsNull() || string.IsNullOrEmpty(m_joinLobbyField.text))
            {
                return;
            }

            CSteamID _inputSteamID = new CSteamID(Convert.ToUInt64(m_joinLobbyField.text));
            OnlineGameController.Instance.JoinLobbyBySteamID(_inputSteamID, OpenLobby);
        }

        public void LobbyEntered(string _lobbyName, bool _isHost)
        {
            m_lobbyName.text = _lobbyName;
            m_lobbyID.text = "Lobby ID: " + OnlineGameController.Instance.currentLobbyID.ToString();
            m_startLobbyButton.SetActive(_isHost);
        }

        public void OnPlayerEnteredLobby(string _userName, Texture _avatar)
        {
            m_joinedPlayerName.text = _userName;
            m_avatarIcon.texture = _avatar;
        }

        public void LeaveLobby()
        {
            OnlineGameController.Instance.LeaveLobby();
            OpenMainMenu();
        }

        public void SettingsPressed()
        {
            if (settingsUIWindowData.IsNull())
            {
                return;
            }
            
            UIUtils.OpenUI(settingsUIWindowData);
        }

        public void ExitPressed()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion


    }
}