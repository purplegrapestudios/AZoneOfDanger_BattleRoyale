using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public struct StormData : INetworkStruct
{
    public int StartPauseTick;
    public int StartStormTick;
    //public bool HasStormPaused;
    //public bool HasStormStarted;
    public float StartScale;
    public float EndSize;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public int TimeStormPause;
    public int TimeStormClose;
    [Space(20), Header("Runtime Data")]
    public float ElapsedPauseTicks;
    public float ElapsedStormTicks;

    public bool TimeStormCloseExpired(NetworkRunner runner) => runner.IsRunning && TimeStormClose / runner.DeltaTime > 0
    && (Tick)(TimeStormClose / runner.DeltaTime) <= runner.Simulation.Tick;
    public bool TimeStormPauseExpired(NetworkRunner runner) => runner.IsRunning && TimeStormPause / runner.DeltaTime > 0
    && (Tick)(TimeStormPause / runner.DeltaTime) <= runner.Simulation.Tick;
}

/// <summary> Storm Behavior Summary </summary>
/// 
/// Stacking Phase: Say 60 Seconds at the start of the game where respawns are allowed, and players stack their armor.
/// 
/// 
/// Storm Timer logic will follow this format: (say 30 seconds wait + 30 seconds closing = 1 min)
/// 1) Display GameStateLabel to Initiate Storm (Minimap Info: Storm Starting in 30 seconds)
/// -> The MinimapCountDownLabel will start counting down.
/// 
/// 2) Display GameStateLabel Storm Closing state (Minimap Info: Storm is closing!)
/// -> The MinimapCountDownLabel will start counting down.
/// 
/// Store Circle Position / Size Logic. (7 Zone sizes each 1 minute = 7 Total Minutes) + Say the first 3 minutes of ffa
/// 1) Position: Storm Circle must be within game map bounds. (Origin + radius) must be within bounds.
/// 2) Size: Storm Size is 100% -> 75% -> 50% -> 25% -> 10% -> 5% -> 0%
/// 
/// End Game Condition
/// When 1 player remains
/// Show Winner Cam
/// Winner Gets UI display of Victory Royale
/// Winner Display Game Stats
/// Those who die early will be spectating
/// Show stats
/// Server waits like 15 seconds for Winner to check his stats etc, and the ShutsDown the Fusion Network, and finally return back to Starting Scene.

public class StormBehavior : NetworkBehaviour
{
    [Networked] Vector3 NetworkedPosition { get; set; }
    [Networked] Vector3 NetworkedScale { get; set; }

    public StormData[] m_stormDatas;
    private List<LagCompensatedHit> m_stormHits;
    [SerializeField] private LayerMask m_playerLayerMask;
    [Networked] public TickTimer GameTimer { get; set; }
    [Networked] public TickTimer StormPauseTimer { get; set; }
    [Networked] public TickTimer StormCloseTimer { get; set; }
    [Networked] public TickTimer StackingPhaseTimer { get; set; }
    [Networked] public int NetworkedStormPhase { get; set; }
    [SerializeField] private int m_stackingTime;
    private TickTimer m_stormDamageTickTimer;
    private float m_tickRate;
    private Dictionary<Player, Character> m_playersInZone;

    private App m_app;
    public static StormBehavior Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        m_app = App.FindInstance();
        if (m_stormHits == null) m_stormHits = new List<LagCompensatedHit>();
        m_playersInZone = new Dictionary<Player, Character>();
        m_tickRate = Mathf.RoundToInt(1 / Runner.DeltaTime);
        NetworkedScale = Vector3.positiveInfinity;
        NetworkedPosition = Vector3.positiveInfinity;
    }

    public bool TickCheck()
    {
        return (Runner.Tick == GameLogicManager.Instance.NetworkedGameStartTick);
    }

    public int NetGameStartTick()
    {
        return GameLogicManager.Instance.NetworkedGameStartTick;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_app.AllowInput) return;
        if (Runner.IsServer) ScanForPlayers(() => DamagePlayersOutsideArea());

        if (Runner.Tick == GameLogicManager.Instance.NetworkedGameStartTick)
        {
            if (Runner.IsServer) StackingPhaseTimer = TickTimer.CreateFromSeconds(Runner, m_stackingTime);
        }
        else if (IsStackingPhaseComplete())
        {
            ShrinkToDataPoint(ref m_stormDatas[NetworkedStormPhase]);
        }
        else
        {
            GameUIViewController.Instance.SetGameStateLabel("Power up your stack");
            GameUIViewController.Instance.SetGameStateTimer(Mathf.FloorToInt(StackingPhaseTimer.RemainingTime(Runner) ?? 0).ToString());
        }
    }

    public bool IsStackingPhaseComplete()
    {
        return (Runner.Tick >= GameLogicManager.Instance.NetworkedGameStartTick + m_tickRate * m_stackingTime);
    }

    private void DamagePlayersOutsideArea()
    {
        if (GameLogicManager.Instance.NetworkedGameIsFinished) return;
        m_app.ForEachPlayer(ply =>
        {
            if (!m_playersInZone.ContainsKey(ply))
            {
                //Debug.Log($"Storm Damaged player: {ply.Character.Id}");
                ply.NetworkedCharacter.CharacterHealth.OnTakeDamage(1, instigator: null);
            }
        });
    }

    private void ScanForPlayers(System.Action damagePlayersCallback)
    {
        if (!Runner.IsServer) return;

        if (m_stormDamageTickTimer.ExpiredOrNotRunning(Runner)) m_stormDamageTickTimer = TickTimer.CreateFromTicks(Runner, (int)m_tickRate);
        if (m_stormDamageTickTimer.RemainingTicks(Runner) != m_tickRate) return;

        if (m_playersInZone != null) m_playersInZone.Clear();
        if (Runner.LagCompensation.OverlapSphere(transform.position, Mathf.Round(transform.localScale.x / 2), player: Object.InputAuthority, m_stormHits, m_playerLayerMask, HitOptions.SubtickAccuracy) > 0)
        {
            foreach (LagCompensatedHit stormHit in m_stormHits)
            {
                var stormHitRoot = stormHit.Hitbox.Root;

                if (stormHit.Hitbox.HitboxIndex > 0) continue;

                if (stormHitRoot.GetComponent<Character>())
                {
                    var stormHitCharacter = stormHitRoot.GetComponent<Character>();
                    //Debug.Log($"Player Inside SafeZone: {stormHitCharacter.Id}, size of Sphere Radius: {transform.localScale.x}");
                    m_playersInZone.Add(stormHitCharacter.Player, stormHitCharacter);
                }
            }
        }
        damagePlayersCallback();
    }

    private void ShrinkToDataPoint(ref StormData data)
    {
        if (data.StartPauseTick == 0)
        {
            data.StartPauseTick = Runner.Tick;
            if (Runner.IsServer)
            {
                StormPauseTimer = TickTimer.None;
                StormPauseTimer = TickTimer.CreateFromTicks(Runner, (int)m_tickRate * data.TimeStormPause);
                //Debug.Log($"Storm is commencing soon. StormPhase: {NetworkedStormPhase}");
            }
        }
        data.ElapsedPauseTicks = Runner.Tick - data.StartPauseTick;
        if (StormPauseTimer.RemainingTicks(Runner) > 0)
        {
            if (StormPauseTimer.RemainingTicks(Runner) == null) return;
            var label = NetworkedStormPhase == m_stormDatas.Length - 1 ? $"Final Zone, {NetworkedStormPhase}, is commencing soon" : $"Zone, {NetworkedStormPhase}, is commencing soon";
            GameUIViewController.Instance.SetGameStateLabel(label);
            GameUIViewController.Instance.SetGameStateTimer(Mathf.CeilToInt(StormPauseTimer.RemainingTime(Runner) ?? 0).ToString());

            return;
        }

        if (data.StartStormTick == 0)
        {
            data.StartStormTick = Runner.Tick;
            if (Runner.IsServer)
            {
                StormCloseTimer = TickTimer.None;
                StormCloseTimer = TickTimer.CreateFromTicks(Runner, (int)m_tickRate * data.TimeStormClose);
            }
            var label = NetworkedStormPhase == m_stormDatas.Length - 1 ? $"The Zone of Danger, is closing!" : $"Zone, {NetworkedStormPhase}, is closing!";
            GameUIViewController.Instance.SetGameStateLabel(label);
        }
        data.ElapsedStormTicks = Runner.Tick - data.StartStormTick;
        GameUIViewController.Instance.SetGameStateTimer(Mathf.CeilToInt(StormCloseTimer.RemainingTime(Runner) ?? 0).ToString());
        var curProg = ((data.TimeStormClose * m_tickRate) - StormCloseTimer.RemainingTicks(Runner) ?? 0) / (data.TimeStormClose * m_tickRate);
        if (Runner.IsServer)
        {
            transform.position = Vector3.Lerp(data.StartPos, data.EndPos, curProg);
            transform.localScale = Vector3.Lerp(new Vector3(data.StartScale, data.StartScale, data.StartScale), new Vector3(data.EndSize, data.EndSize, data.EndSize), curProg);
            NetworkedPosition = transform.position;
            NetworkedScale = transform.localScale;
        }
        else
        {
            if (StormPauseTimer.RemainingTicks(Runner) == null) return;
            if (StormCloseTimer.RemainingTicks(Runner) == null) return;
            var label = NetworkedStormPhase == m_stormDatas.Length - 1 ? $"The Zone of Danger, is closing!" : $"Zone, {NetworkedStormPhase}, is closing!";
            GameUIViewController.Instance.SetGameStateLabel(label);
            transform.position = NetworkedPosition;
            transform.localScale = NetworkedScale;
        }

        if (curProg >= 1)
        {
            if (Runner.IsServer)
            {
                if (NetworkedStormPhase < m_stormDatas.Length - 1)
                    NetworkedStormPhase++;
            }
        }
    }
}