using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq.Expressions;
using System;

public class GameLogicManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static GameLogicManager Instance;
    [Networked] public NetworkBool NetworkedGameIsRunning { get; set; }
    [Networked] public NetworkBool NetworkedGameIsFinished { get; set; }
    [Networked] public int NetworkedGameStartTick { get; set; }
    [Networked] public NetworkBool NetworkedRespawnAllowed { get; set; }
    [Networked] public Player NetworkedVictoryPlayer { get; set; }
    public int MinPlayersToStart => m_minPlayersToStart;
    [SerializeField] private int m_minPlayersToStart = 2;

    public int SpectatePlayerIndex => m_spectatePlayerIndex;
    [SerializeField] private int m_spectatePlayerIndex;
    public Player PlayerCurrentlySpectating;
    [SerializeField] private Player m_playerCurrentlySpectating;

    [Networked, Capacity(200)] public NetworkDictionary<int, PlayerRef> NetworkedPlayerDictionary => default;
    public int kVictoryPlayerCount = 1;
    public bool Initialized => m_initialized;
    private bool m_initialized;
    private App m_app;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        m_app = App.FindInstance();
        GameUIViewController.Instance.InitSpectatePlayerButtons(m_app);
        GameUIViewController.Instance.InitVictoryScreen();

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
        Debug.Log($"Player: {plyRef.PlayerId} joined the game");
        if (!Runner.IsServer) return;

        if (NetworkedPlayerDictionary.Contains(new KeyValuePair<int, PlayerRef>(plyRef.PlayerId, plyRef))) return;

        NetworkedPlayerDictionary.Add(plyRef.PlayerId, plyRef);
    }

    void IPlayerLeft.PlayerLeft(PlayerRef plyRef)
    {
        Debug.Log($"Player: {plyRef.PlayerId} left the game");
        if (!Runner.IsServer) return;

        if (NetworkedPlayerDictionary.Contains(new KeyValuePair<int, PlayerRef>(plyRef.PlayerId, plyRef))) return;

        if (NetworkedPlayerDictionary.Count > kVictoryPlayerCount)
        {
            NetworkedPlayerDictionary.Remove(plyRef.PlayerId);
        }
    }

    public void StartGameLogic()
    {
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

        if (!Runner.IsServer) return;
        NetworkedRespawnAllowed = !StormBehavior.Instance.IsStackingPhaseComplete();

        if (NetworkedRespawnAllowed) return;
        if (NetworkedPlayerDictionary.Count > 0) return;
        //Shut Down the game after X seconds./
    }

    public Player GetSpectatePlayer(int spectatePlayerIndex, Player localPlayer, int fetchAttempts = 0, bool fetchPrevIfLocalUser = false)
    {
        if (NetworkedPlayerDictionary.Count == 0) return null;

        m_spectatePlayerIndex = ClampSpectatePlayerIndex(spectatePlayerIndex);
        Debug.Log($"Spectate index: {m_spectatePlayerIndex}");
        m_playerCurrentlySpectating = Runner.GetPlayerObject(NetworkedPlayerDictionary.ElementAt(m_spectatePlayerIndex).Value)?.GetComponent<Player>() ?? null;

        if (fetchAttempts > 0) return m_playerCurrentlySpectating;
        if (m_playerCurrentlySpectating == localPlayer)
        {
            fetchAttempts++;
            GetSpectatePlayer(fetchPrevIfLocalUser ? m_spectatePlayerIndex - 1 : m_spectatePlayerIndex + 1, localPlayer, fetchAttempts);
        }

        return m_playerCurrentlySpectating;
    }

    public void GetSpectatePlayerNext(Player localPlayer)
    {
        if (NetworkedPlayerDictionary.Count == 0) return;

        m_spectatePlayerIndex++;
        m_spectatePlayerIndex = ClampSpectatePlayerIndex(m_spectatePlayerIndex);
        var p = GetSpectatePlayer(m_spectatePlayerIndex, localPlayer);
        if (p == null) return;
        SceneCamera.Instance.SetSpectateCamTransform(p.NetworkedCharacter.CharacterCamera.transform, $"Spectating player {p.Object.InputAuthority.PlayerId}");
    }

    public void GetSpectatePlayerPrev(Player localPlayer)
    {
        if (NetworkedPlayerDictionary.Count == 0) return;

        m_spectatePlayerIndex--;
        m_spectatePlayerIndex = ClampSpectatePlayerIndex(m_spectatePlayerIndex);
        var p = GetSpectatePlayer(m_spectatePlayerIndex, localPlayer, fetchPrevIfLocalUser: true);
        if (p == null) return;
        SceneCamera.Instance.SetSpectateCamTransform(p.NetworkedCharacter.CharacterCamera.transform, $"Spectating player {p.Object.InputAuthority.PlayerId}");
    }

    private int ClampSpectatePlayerIndex(int spectateIndex)
    {
        if (spectateIndex > NetworkedPlayerDictionary.Count - 1) spectateIndex = 0;
        if (spectateIndex < 0) spectateIndex = NetworkedPlayerDictionary.Count - 1;
        return spectateIndex;
    }
}