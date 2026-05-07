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
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("Scene Transition")]
    [Tooltip("Scene to load on the host after creating the lobby. Clients auto-follow.")]
    public string mapSceneName = "Map";

    const string JOIN_CODE_KEY = "JoinCode";
    const float HEARTBEAT_INTERVAL = 15f;

    public Lobby CurrentLobby { get; private set; }
    public bool IsHost { get; private set; }
    public string PlayerName { get; private set; }
    public bool ServicesReady { get; private set; }

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
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            ServicesReady = true;
            Debug.Log($"[Lobby] Signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Lobby] Services init failed: {e.Message}\n" +
                           "Check: Project linked to Unity Cloud + Lobby/Relay launched at cloud.unity.com");
        }
    }

    public async Task<bool> CreateLobby(string lobbyName, string playerName, int maxPlayers = 4)
    {
        if (!ServicesReady)
        {
            Debug.LogError("[Lobby] Services not ready yet. Wait a moment after Play, then try again.");
            return false;
        }

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

            // Move host to the map scene; clients will follow automatically when they join.
            if (!string.IsNullOrEmpty(mapSceneName))
                NetworkManager.Singleton.SceneManager.LoadScene(mapSceneName, LoadSceneMode.Single);

            Debug.Log($"[Lobby] Created '{lobbyName}' — RelayCode={joinCode} LobbyId={CurrentLobby.Id}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Lobby] Create failed: {e.GetType().Name} — {e.Message}");
            return false;
        }
    }

    public async Task<bool> JoinLobbyByName(string lobbyName, string playerName)
    {
        if (!ServicesReady)
        {
            Debug.LogError("[Lobby] Services not ready yet. Wait a moment after Play, then try again.");
            return false;
        }

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
            Debug.LogError($"[Lobby] Join failed: {e.GetType().Name} — {e.Message}");
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
        catch { }
    }
}
