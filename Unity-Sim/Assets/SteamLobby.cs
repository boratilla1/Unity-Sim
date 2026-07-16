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

    [Header("UI Referanslarý")]
    public GameObject HostButton;
    public TextMeshProUGUI LobbyNameText;

    [Header("Sahne Geçiþi")]
    public GameObject StartGameButton;
    public string sceneToLoad = "TestScene";

    public void HostLobby()
    {
        Debug.Log($"<color=yellow>[SteamLobby]</color> HostLobby týklandý. Manager null mu? {manager == null}");
        if (manager == null) return;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    public void StartGame()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("<color=red>[SteamLobby]</color> Sadece lobi kurucusu (Host) oyunu baþlatabilir!");
            return;
        }

        Debug.Log($"<color=magenta>[SteamLobby]</color> Host tarafýndan StartGame tetiklendi! Herkes {sceneToLoad} sahnesine çekiliyor...");
        manager.ServerChangeScene(sceneToLoad);
    }

    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        manager = GetComponent<CustomNetworkManager>();
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnjoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        if (StartGameButton != null) StartGameButton.SetActive(false);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }
        Debug.Log("<color=yellow>[SteamLobby]</color> Lobi baþarýyla oluþturuldu, StartHost tetikleniyor.");
        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'S LOBBY");
    }

    private void OnjoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log($"<color=yellow>[SteamLobby]</color> Join request geldi, lobby id: {callback.m_steamIDLobby}");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log($"<color=yellow>[SteamLobby]</color> Lobiye girildi! Lobby ID: {callback.m_ulSteamIDLobby}");
        HostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        LobbyNameText.gameObject.SetActive(true);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        if (NetworkServer.active)
        {
            Debug.Log("<color=yellow>[SteamLobby]</color> Ben Host'um. Start Game butonunu aktif ediyorum.");
            if (StartGameButton != null) StartGameButton.SetActive(true);
            return;
        }

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        Debug.Log($"<color=yellow>[SteamLobby]</color> Client baðlanýyor, adres: {manager.networkAddress}");
        manager.StartClient();
    }
}