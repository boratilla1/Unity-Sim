using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    public GameObject HostButton;
    public TextMeshProUGUI LobbyNameText;

    public void HostLobby()
    {
        Debug.Log("HostLobby called, manager null mu? " + (manager == null));
        if (manager == null) return;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }


    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        manager = GetComponent<CustomNetworkManager>();
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnjoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }
        Debug.Log("lobby created");
        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'S LOBBY");
    }

    private void OnjoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Join request geldi, lobby id: " + callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("OnLobbyEntered çalýþtý, lobby id: " + callback.m_ulSteamIDLobby);
        HostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        LobbyNameText.gameObject.SetActive(true);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
        Debug.Log("Lobby name text: " + LobbyNameText.text);
        if (NetworkServer.active) { return; }
        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        Debug.Log("Client baglaniyor, adres: " + manager.networkAddress);
        manager.StartClient();
    }
}
