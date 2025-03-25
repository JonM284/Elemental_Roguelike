using System;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using HeathenEngineering.SteamworksIntegration;
using Project.Scripts.Utils;
using Runtime.UI.DataModels;
using Steamworks;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class OnlineGameController: GameControllerBase
    {

        #region Static

        public static OnlineGameController Instance { get; private set; }
        
        #endregion

        #region Serialized Fields

        [SerializeField] private NetworkManager m_networkManager;

        [SerializeField] private FishySteamworks.FishySteamworks m_fishySteamworks;

        #endregion

        #region Private Fields

        private List<UserData> m_lobbyUsers = new List<UserData>();

        #endregion

        #region Steam Callbacks

        protected Callback<LobbyCreated_t> LobbyCreated;

        protected Callback<GameLobbyJoinRequested_t> JoinRequest;

        protected Callback<LobbyEnter_t> LobbyEntered;

        protected Callback<LobbyChatMsg_t> LobbyChatMessage;

        #endregion

        #region Accessors

        public ulong currentLobbyID { get; private set; }

        #endregion
        
        
        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
            SetupController();
        }

        #endregion

        #region Class Implementation

        private void SetupController()
        {
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
        
        
        public void OnInitialized()
        {
            
            //Allow for multiplayer button
            
            
        }


        public void OnFailedInitialize()
        {
            //Don't allow for multiplayer
            
            //Display quick popup notification that states error
            
        }

        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
        }

        private void OnLobbyCreated(LobbyCreated_t _callback)
        {
            if (_callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }
            
            currentLobbyID = _callback.m_ulSteamIDLobby;

            SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
            
            m_fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
            m_fishySteamworks.StartConnection(true);
            
            Debug.Log("<color=cyan> Lobby Created </color>");
        }

        private void OnJoinRequest(GameLobbyJoinRequested_t _callback)
        {
            SteamMatchmaking.JoinLobby(_callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t _callback)
        {
            currentLobbyID = _callback.m_ulSteamIDLobby;

            if (!MainMenuDataModel.Instance.IsNull())
            {
                MainMenuDataModel.Instance.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "name"), m_networkManager.IsServer);
            }
            
            m_fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "HostAddress"));
            m_fishySteamworks.StartConnection(false);
            
            Debug.Log("<color=cyan> Lobby Entered </color>");
        }
        
        public void JoinLobbyBySteamID(CSteamID _steamID, Action _callback = null)
        {
            Debug.Log($"Attempting to join lobby with ID: {_steamID}");

            if (SteamMatchmaking.RequestLobbyData(_steamID))
            {
                SteamMatchmaking.JoinLobby(_steamID);
                _callback?.Invoke();
            }
            else
            {
                Debug.Log($"Failed to Join Lobby with ID {_steamID}");
            }
            
        }

        public void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
            
            currentLobbyID = 0;

            m_fishySteamworks.StopConnection(false);

            if (m_networkManager.IsServer)
            {
                m_fishySteamworks.StopConnection(true);
            }

        }

        #endregion


    }
}