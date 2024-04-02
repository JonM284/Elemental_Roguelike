using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using Networking;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{

    #region Instance

    private static BootstrapManager Instance;

    #endregion

    #region Serialized Fields

    [SerializeField] private string menuName = "MultiplayerMenu";

    [SerializeField] private NetworkManager m_networkManager;

    [SerializeField] private FishySteamworks.FishySteamworks m_fishySteamworks;

    #endregion

    #region Private Fields

    public static ulong CurrentLobbyID { get; private set; }

    #endregion

    #region Callbacks

    protected Callback<LobbyCreated_t> LobbyCreated;
    
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    
    protected Callback<LobbyEnter_t> LobbyEntered;
    
    #endregion

    #region Unity Events

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    #endregion

    #region Class Implementation

    public void ChangeScene()
    {
        SceneManager.LoadScene(menuName, LoadSceneMode.Additive);
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }
    
    private void OnLobbyCreated(LobbyCreated_t _callback)
    {
        Debug.Log("Started Lobby Creation: " + _callback.m_eResult.ToString());
        if (_callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        CurrentLobbyID = _callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName() + "'s lobby");
        m_fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        m_fishySteamworks.StartConnection(true);
        
        Debug.Log("<color=#00FF00> Lobby Creation Successful </color>");
    }
    

    private void OnJoinRequest(GameLobbyJoinRequested_t _callback)
    {
        SteamMatchmaking.JoinLobby(_callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t _callback)
    {
        CurrentLobbyID = _callback.m_ulSteamIDLobby;

        ConnectionManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name"), m_networkManager.IsServerStarted);
        
        m_fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        m_fishySteamworks.StartConnection(false);
    }

    public static void JoinByID(CSteamID _steamID)
    {
        Debug.Log("Attempting to join lobby with ID: " +_steamID.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(_steamID))
        {
            SteamMatchmaking.JoinLobby(_steamID);
        }
        else
        {
            Debug.Log("Failed to join lobby with ID: " +_steamID.m_SteamID);
        }
    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;

        Instance.m_fishySteamworks.StopConnection(false);
        if (Instance.m_networkManager.IsServerStarted)
        {
            Instance.m_fishySteamworks.StopConnection(true);
        }
    }

    #endregion

}
