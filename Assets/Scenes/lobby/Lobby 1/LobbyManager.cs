using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    const string JOIN_CODE_KEY = "JoinCode";
    const float HEARTBEAT_INTERVAL = 15f;

    public Lobby CurrentLobby { get; private set; }
    public bool IsHost { get; private set; }
    public string PlayerName { get; private set; }

    Coroutine heartbeatRoutine;

    async void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitServices();
    }

    async Task InitServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"[Lobby] Signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");
    }

    public async Task<bool> CreateLobby(string lobbyName, string playerName, int maxPlayers = 4)
    {
        try
        {
            PlayerName = playerName;

            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = BuildPlayer(playerName),
                Data = new Dictionary<string, DataObject>
                {
                    { JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            IsHost = true;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            heartbeatRoutine = StartCoroutine(HeartbeatCoroutine(CurrentLobby.Id));

            Debug.Log($"[Lobby] Created '{lobbyName}' — RelayCode={joinCode} LobbyId={CurrentLobby.Id}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Lobby] Create failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> JoinLobbyByName(string lobbyName, string playerName)
    {
        try
        {
            PlayerName = playerName;

            var query = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.EQ)
                }
            };
            var results = await LobbyService.Instance.QueryLobbiesAsync(query);
            if (results.Results.Count == 0)
            {
                Debug.LogWarning($"[Lobby] No lobby found with name '{lobbyName}'");
                return false;
            }

            var joinOptions = new JoinLobbyByIdOptions { Player = BuildPlayer(playerName) };
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(results.Results[0].Id, joinOptions);
            IsHost = false;

            var joinCode = CurrentLobby.Data[JOIN_CODE_KEY].Value;
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            Debug.Log($"[Lobby] Joined '{lobbyName}'");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Lobby] Join failed: {e.Message}");
            return false;
        }
    }

    Player BuildPlayer(string name) => new Player(
        id: AuthenticationService.Instance.PlayerId,
        data: new Dictionary<string, PlayerDataObject>
        {
            { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name) }
        });

    IEnumerator HeartbeatCoroutine(string lobbyId)
    {
        var wait = new WaitForSecondsRealtime(HEARTBEAT_INTERVAL);
        while (true)
        {
            yield return wait;
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
    }

    async void OnApplicationQuit()
    {
        if (heartbeatRoutine != null) StopCoroutine(heartbeatRoutine);
        if (CurrentLobby == null) return;

        try
        {
            if (IsHost) await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
            else        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch { /* shutting down; ignore */ }
    }
}
