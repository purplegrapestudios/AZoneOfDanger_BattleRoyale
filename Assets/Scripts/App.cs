using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using GameUI;
using GameUI.Intro;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct SessionRequest
{
	public string UserID;
	public GameMode GameMode;
	public string DisplayName;
	public string SessionName;
	public string ScenePath;
	public int MaxPlayers;
	public int ExtraPeers;
	public string CustomLobby;
	public string IPAddress;
	public ushort Port;
}

public enum ConnectionStatus
{
	Disconnected,
	Connecting,
	Connected,
	Failed,
	EnteringLobby,
	InLobby,
	Starting,
	Started
}

/// <summary>
/// This is the main entry point for the application. App is a singleton created when the game is launched.
/// Access it anywhere using `App.Instance`
/// </summary>

[RequireComponent(typeof(NetworkSceneManagerBase))]
public class App : MonoBehaviour, INetworkRunnerCallbacks
{
	[SerializeField] private SceneReference _introScene;
	[SerializeField] private Player _playerPrefab;
	[SerializeField] private Session _sessionPrefab;
	[SerializeField] private ErrorBox _errorBox;
	[SerializeField] private PlayerSetupPanel _playerSetup;
	[SerializeField] private bool _sharedMode;

	[Space(10)]
	[SerializeField] private bool _autoConnect;
	[SerializeField] private bool _skipStaging;
	[SerializeField] private SessionProps _autoSession = new SessionProps();

	private NetworkRunner _runner;
	private NetworkSceneManagerBase _loader;
	private Action<List<SessionInfo>> _onSessionListUpdated;
	private InputData _data;
	private Session _session;
	private string _lobbyId;
	private bool _allowInput;

	public static App FindInstance()
	{
		return FindObjectOfType<App>();
	}

	public ConnectionStatus ConnectionStatus { get; private set; }
	public bool IsSessionOwner => _runner != null && (_runner.IsServer || _runner.IsSharedModeMasterClient);
	public SessionProps AutoSession => _autoSession;
	public bool SkipStaging => _skipStaging;

	public bool AllowInput
	{
		get => _allowInput && Session != null && Session.PostLoadCountDown.Expired(Session.Runner);
		set => _allowInput = value;
	} 

	public bool IsBatchMode()
    {
		return (Application.isBatchMode);
	}

	public bool IsHost()
    {
		return Session.Runner.GameMode == GameMode.Host;
    }

	private void Awake()
	{
		App[] apps = FindObjectsOfType<App>();
		if (apps != null && apps.Length > 1)
		{
			// If there is more than one App, then surely we must be an extra, so go jump off a cliff
			Destroy(gameObject);
			return;
		}
		
		if(_loader==null)
		{
			_loader = GetComponent<NetworkSceneManagerBase>();
		
			DontDestroyOnLoad(gameObject);

			if (_autoConnect)
			{
				StartSession( _sharedMode ? GameMode.Shared : GameMode.AutoHostOrClient, _autoSession, false);
			}
			else
			{
				SceneManager.LoadSceneAsync( _introScene );
			}
		}
	}

	private void Connect()
	{
		if (_runner == null)
		{
			SetConnectionStatus(ConnectionStatus.Connecting);
			GameObject go = new GameObject("Session");
			go.transform.SetParent(transform);

			_runner = go.AddComponent<NetworkRunner>();
			_runner.AddCallbacks(this);
		}
	}

	public void Disconnect()
	{
		if (_runner != null)
		{
			Debug.Log("We are shutting down Photon Fusion Network");
			SetConnectionStatus(ConnectionStatus.Disconnected);
			_runner.Shutdown();
		}
	}

	public void JoinSession(SessionInfo info)
	{
		SessionProps props = new SessionProps(info.Properties);
		props.PlayerLimit = info.MaxPlayers;
		props.RoomName = info.Name;
		StartSession(_sharedMode ? GameMode.Shared : GameMode.Client, props);
	}
	
	public void CreateSession(SessionProps props, bool useHostInsteadOfServer)
	{
		if(useHostInsteadOfServer)
			StartSession(_sharedMode ? GameMode.Shared : GameMode.Host, props, !_sharedMode);
		else
			StartSession(_sharedMode ? GameMode.Shared : GameMode.Server, props, !_sharedMode);
	}

	private async void StartSession(GameMode mode, SessionProps props, bool disableClientSessionCreation=true)
	{
		Connect();

		SetConnectionStatus(ConnectionStatus.Starting);

		Debug.Log($"Starting game with session {props.RoomName}, player limit {props.PlayerLimit}");

		_runner.ProvideInput = mode != GameMode.Server;

		StartGameArgs startGameArgs = new StartGameArgs
		{
			GameMode = mode,
			CustomLobbyName = _lobbyId,
			SceneManager = _loader,
			SessionName = props.RoomName,
			PlayerCount = props.PlayerLimit,
			SessionProperties = props.Properties,
			DisableClientSessionCreation = disableClientSessionCreation,
			Address = NetAddress.Any(),
		};


		//startGameArgs.Address = NetAddress.CreateFromIpPort(ServerConfigData.IPAddress, ServerConfigData.Port);
		Debug.Log($"IP Address and port: {startGameArgs.Address}");
		StartGameResult result = await _runner.StartGame(startGameArgs);
		if(!result.Ok)
			SetConnectionStatus(ConnectionStatus.Failed, result.ShutdownReason.ToString());
		
		if(result.Ok)
			Debug.LogWarning($"{_runner.AuthenticationValues.UserId}, {(_runner.Simulation == null ? "null" : "simulation exists")} started on connectionType {_runner.CurrentConnectionType} in {(Time.realtimeSinceStartup):0.00}s");
	}

	public async Task EnterLobby(string lobbyId, Action<List<SessionInfo>> onSessionListUpdated)
	{
		Connect();

		_lobbyId = lobbyId;
		_onSessionListUpdated = onSessionListUpdated;

		SetConnectionStatus(ConnectionStatus.EnteringLobby);
		var result = await _runner.JoinSessionLobby(SessionLobby.Custom, lobbyId);

		if (!result.Ok) {
			_onSessionListUpdated = null;
			SetConnectionStatus(ConnectionStatus.Failed);
			onSessionListUpdated(null);
		}
	}

	public Session Session
	{
		get => _session;
		set { _session = value; _session.transform.SetParent(_runner.transform); }
	}

	public Player GetPlayer()
	{
		return _runner?.GetPlayerObject(_runner.LocalPlayer)?.GetComponent<Player>();
	}
	
	public void ForEachPlayer(Action<Player> action)
	{
		foreach (PlayerRef plyRef in _runner.ActivePlayers)
		{
			NetworkObject plyObj = _runner.GetPlayerObject(plyRef);
			if (plyObj)
			{
				Player ply = plyObj.GetComponent<Player>();
				action(ply);
			}
		}
	}

	private void SetConnectionStatus(ConnectionStatus status, string reason="")
	{
		if (ConnectionStatus == status)
			return;
		ConnectionStatus = status;

		if (!string.IsNullOrWhiteSpace(reason) && reason != "Ok")
		{
			_errorBox.Show(status,reason);
		}
		
		Debug.Log($"ConnectionStatus={status} {reason}");
	}
	
	/// <summary>
	/// Fusion Event Handlers
	/// </summary>

	public void OnConnectedToServer(NetworkRunner runner)
	{
		Debug.Log("Connected to server");
		SetConnectionStatus(ConnectionStatus.Connected);
	}

	public void OnDisconnectedFromServer(NetworkRunner runner)
	{
		Debug.Log("Disconnected from server");
		Disconnect();
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
		Debug.Log($"Connect failed {reason}");
		Disconnect();
		SetConnectionStatus(ConnectionStatus.Failed, reason.ToString());
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
	{
		Debug.Log($"Player {playerRef} Joined!");
		if ( _session==null && IsSessionOwner)
		{
			Debug.Log("Spawning world");
			_session = runner.Spawn(_sessionPrefab, Vector3.zero, Quaternion.identity);
		}

		if (runner.IsServer || runner.Topology == SimulationConfig.Topologies.Shared && playerRef == runner.LocalPlayer)
		{
			Debug.Log("Spawning player");
			runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, playerRef, (runner, obj) => runner.SetPlayerObject(playerRef, obj) );
		}

		SetConnectionStatus(ConnectionStatus.Started);
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"{player.PlayerId} disconnected.");

		if (runner.IsServer)
		{
			NetworkObject playerObj = runner.GetPlayerObject(player);
			if (playerObj)
			{
				if (playerObj != null && playerObj.HasStateAuthority)
				{
					Debug.Log("Despawning Player");
					playerObj.GetComponent<Player>().Despawn();
				}
			}
		}
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason reason)
	{
		Debug.Log($"OnShutdown {reason}");
		SetConnectionStatus(ConnectionStatus.Disconnected, reason.ToString());

		if(_runner!=null && _runner.gameObject)
			Destroy(_runner.gameObject);

		_runner = null;
		_session = null;

		if(Application.isPlaying)
			SceneManager.LoadSceneAsync(_introScene);
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
		request.Accept();
	}

	private void Update() 
	{
		// Check events like KeyDown or KeyUp in Unity's update. They might be missed otherwise because they're only true for 1 frame
		_data.ButtonFlags |= Input.GetKeyDown( KeyCode.R ) ? ButtonFlag.RESPAWN : 0;
	}
	
	public void ShowPlayerSetup()
	{
		if(_playerSetup)
			_playerSetup.Show(true);
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		if (!AllowInput)
			return;

		// Persistent button flags like GetKey should be read when needed so they always have the actual state for this tick
		_data.ButtonFlags |= Input.GetKey( KeyCode.W ) ? ButtonFlag.FORWARD : 0;
		_data.ButtonFlags |= Input.GetKey( KeyCode.A ) ? ButtonFlag.LEFT : 0;
		_data.ButtonFlags |= Input.GetKey( KeyCode.S ) ? ButtonFlag.BACKWARD : 0;
		_data.ButtonFlags |= Input.GetKey(KeyCode.D) ? ButtonFlag.RIGHT : 0;
		_data.ButtonFlags |= Input.GetKey(KeyCode.Space) ? ButtonFlag.JUMP : 0;
		_data.aimDirection = new Vector2(Input.GetAxis("Mouse X") * 10, Input.GetAxis("Mouse Y") * 10);
		input.Set( _data );

		// Clear the flags so they don't spill over into the next tick unless they're still valid input.
		_data.ButtonFlags = 0;
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		SetConnectionStatus(ConnectionStatus.InLobby);
		_onSessionListUpdated?.Invoke(sessionList);
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
}