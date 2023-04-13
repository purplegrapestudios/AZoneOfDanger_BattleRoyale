using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameLogicManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static GameLogicManager Instance;
    [Networked] public NetworkBool NetworkedGameIsRunning { get; set; }
    [Networked] public int NetworkedGameStartTick { get; set; }
    [Networked] public NetworkBool NetworkedRespawnAllowed { get; set; }
    public int MinPlayersToStart => m_minPlayersToStart;
    [SerializeField] private int m_minPlayersToStart = 2;

    public int SpectatePlayerIndex => m_spectatePlayerIndex;
    [SerializeField] private int m_spectatePlayerIndex;
    public Player PlayerCurrentlySpectating;
    [SerializeField] private Player m_playerCurrentlySpectating;

    public int SpecCamCountDebug;
    [Networked, Capacity(200)] public NetworkDictionary<int, PlayerRef> NetworkedPlayerDictionary => default;
    private App m_app;
    public bool Initialized => m_initialized;
    private bool m_initialized;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        m_app = App.FindInstance();
        GameUIViewController.Instance.InitSpectatePlayerButtons(m_app);
        m_initialized = true;

        if (!Runner.IsServer) return;

        //Pre-populate this dictionary at the beginning;
        foreach (PlayerRef plyRef in Runner.ActivePlayers)
        {
            if (NetworkedPlayerDictionary.Contains(new KeyValuePair<int, PlayerRef>(plyRef.PlayerId, plyRef))) continue;
            NetworkedPlayerDictionary.Add(plyRef.PlayerId, plyRef);
            Debug.Log($"Player: {plyRef.PlayerId} added to PlayerDictionary. (Already in the game)");
        }
    }

    void IPlayerJoined.PlayerJoined(PlayerRef plyRef)
    {
        if (NetworkedPlayerDictionary.Contains(new KeyValuePair<int, PlayerRef>(plyRef.PlayerId, plyRef))) return;

        NetworkedPlayerDictionary.Add(plyRef.PlayerId, plyRef);
        Debug.Log($"Player: {plyRef.PlayerId} joined the game");
    }

    void IPlayerLeft.PlayerLeft(PlayerRef plyRef)
    {
        if (NetworkedPlayerDictionary.Contains(new KeyValuePair<int, PlayerRef>(plyRef.PlayerId, plyRef))) return;

        NetworkedPlayerDictionary.Remove(plyRef);
        Debug.Log($"Player: {plyRef.PlayerId} left the game");
    }

    public void StartGameLogic()
    {
        //if (m_app.Session.Info.PlayerCount - (m_app.IsServerMode() ? 0 : 0) < m_minPlayersToStart) return;
        if (!Runner.IsServer) return;
        if (NetworkedGameIsRunning) return;

        NetworkedGameStartTick = m_app.Session.Runner.Tick;
        NetworkedGameIsRunning = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_app.AllowInput) return;
        if (!NetworkedGameIsRunning) return;
        GameUIViewController.Instance.FixedUpdateMinimapTime(NetworkedGameStartTick);
        NetworkedRespawnAllowed = !StormBehavior.Instance.IsStackingPhaseComplete();

        SpecCamCountDebug = NetworkedPlayerDictionary.Count;
    }

    public Player GetSpectatePlayer(int spectatePlayerIndex, Player localPlayer, int fetchAttempts = 0, bool fetchPrevIfLocalUser = false)
    {
        m_spectatePlayerIndex = ClampSpectatePlayerIndex(spectatePlayerIndex);

        m_playerCurrentlySpectating = Runner.GetPlayerObject(NetworkedPlayerDictionary.ElementAt(m_spectatePlayerIndex).Value).GetComponent<Player>();

        if (fetchAttempts > 0) return m_playerCurrentlySpectating;
        if (m_playerCurrentlySpectating == localPlayer) 
            GetSpectatePlayer(fetchPrevIfLocalUser ? m_spectatePlayerIndex - 1 : m_spectatePlayerIndex + 1, localPlayer, fetchAttempts++);

        return m_playerCurrentlySpectating;
    }

    public void GetSpectatePlayerNext(Player localPlayer)
    {
        m_spectatePlayerIndex++;
        m_spectatePlayerIndex = ClampSpectatePlayerIndex(m_spectatePlayerIndex);
        var p = GetSpectatePlayer(m_spectatePlayerIndex, localPlayer);
        SceneCamera.Instance.SetSpectateCamTransform(p.NetworkedCharacter.CharacterCamera.transform, $"Spectating player {p.Id}");
    }

    public void GetSpectatePlayerPrev(Player localPlayer)
    {
        m_spectatePlayerIndex--;
        m_spectatePlayerIndex = ClampSpectatePlayerIndex(m_spectatePlayerIndex);
        var p = GetSpectatePlayer(m_spectatePlayerIndex, localPlayer, fetchPrevIfLocalUser: true);
        SceneCamera.Instance.SetSpectateCamTransform(p.NetworkedCharacter.CharacterCamera.transform, $"Spectating player {p.Id}");
    }

    private int ClampSpectatePlayerIndex(int spectateIndex)
    {
        if (spectateIndex > NetworkedPlayerDictionary.Count - 1) spectateIndex = 0;
        if (spectateIndex < 0) spectateIndex = NetworkedPlayerDictionary.Count - 1;
        return spectateIndex;
    }
}