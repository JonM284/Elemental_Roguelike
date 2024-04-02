using System;
using UnityEngine;
using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using TMPro;
using UnityEngine.UI;

namespace Networking
{
    public class ConnectionManager: MonoBehaviour
    {
        
        [SerializeField] private TMP_InputField m_inputField;

        [SerializeField] private GameObject menuScreen, lobbyScreen;

        [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;

        [SerializeField] private Button startGameButton;

        [SerializeField] private RawImage m_userImage;
        
        private string m_hostHex;

        private static ConnectionManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            OpenMainMenu();
        }


        public void OpenMainMenu()
        {
            CloseAllScreens();
            menuScreen.SetActive(true);
        }

        public void OpenLobby()
        {
            CloseAllScreens();
            lobbyScreen.SetActive(true);
        }

        public static void LobbyEntered(string lobbyName, bool isHost)
        {
            Instance.lobbyTitle.text = lobbyName;
            Instance.startGameButton.gameObject.SetActive(isHost);
            Instance.lobbyIDText.text = "Lobby ID: \r\n " + BootstrapManager.CurrentLobbyID.ToString();
            Instance.OpenLobby();
        }
        
        private void CreateLobby()
        {
            OpenLobby();
            BootstrapManager.CreateLobby();
        }

        void CloseAllScreens()
        {
            menuScreen.SetActive(false);
            lobbyScreen.SetActive(false);
        }

        public void LeaveLobby()
        {
            OpenMainMenu();
            BootstrapManager.LeaveLobby();
        }

        public void JoinLobby()
        {
            CSteamID _steamID = new CSteamID(Convert.ToUInt64(m_inputField.text));
            BootstrapManager.JoinByID(_steamID);
        }
        
        
        
    }
}