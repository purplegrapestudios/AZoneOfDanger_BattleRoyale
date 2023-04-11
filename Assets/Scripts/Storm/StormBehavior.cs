using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public struct StormData
{
    public Vector3 EndPos;
    public int TimeToClose;
    public int WaitTime;
    public float ElapsedTime;
    public float ShrinkTime;
    public float Size;
}

public class StormBehavior : NetworkBehaviour
{
    public int StormIntervalTime;
    public int StormCloseTime;
    public StormData[] m_stormDatas;
    private List<LagCompensatedHit> m_stormHits;
    [SerializeField] private LayerMask m_playerLayerMask;
    [Networked] Vector3 NetworkedPosition { get; set; }
    [Networked] Vector3 NetworkedScale { get; set; }
    private TickTimer m_stormDamageTickTimer;
    private TickTimer m_stormShrinkWaitTimer;
    public int CurrentStormPhase;
    private int m_ticksPerSecond;
    private Dictionary<Player, Character> m_playersInZone;
    private List<Player> m_playersNotInZone;

    private System.Action<float, CharacterHealthComponent> m_damageCallback;
    private App m_app;


    public override void Spawned()
    {
        m_app = App.FindInstance();
        if (m_stormHits == null) m_stormHits = new List<LagCompensatedHit>();
        m_playersInZone = new Dictionary<Player, Character>();
        m_ticksPerSecond = Mathf.RoundToInt(1 / Runner.DeltaTime);
    }

    public override void FixedUpdateNetwork()
    {
        ScanForPlayers(() => DamagePlayersOutsideArea());
        ShrinkToDataPoint(ref m_stormDatas[CurrentStormPhase]);
    }

    public void DamagePlayersOutsideArea()
    {
        m_app.ForEachPlayer(ply =>
        {
            if (!m_playersInZone.ContainsKey(ply))
            {
                Debug.Log($"Storm Damaged player: {ply.Character.Id}");
                ply.Character.CharacterHealth.OnTakeDamage(1, instigator: null);
            }
        });
    }

    public void ScanForPlayers(System.Action damagePlayersCallback)
    {
        if (m_stormDamageTickTimer.ExpiredOrNotRunning(Runner)) m_stormDamageTickTimer = TickTimer.CreateFromTicks(Runner, m_ticksPerSecond);
        if (m_stormDamageTickTimer.RemainingTicks(Runner) != m_ticksPerSecond) return;

        if (m_playersInZone != null) m_playersInZone.Clear();
        if (Runner.LagCompensation.OverlapSphere(transform.position, Mathf.Round(transform.localScale.x/2), player: Object.InputAuthority, m_stormHits, m_playerLayerMask, HitOptions.SubtickAccuracy) > 0)
        {
            foreach (LagCompensatedHit stormHit in m_stormHits)
            {
                var stormHitRoot = stormHit.Hitbox.Root;
                
                if (stormHit.Hitbox.HitboxIndex > 0) continue;

                if (stormHitRoot.GetComponent<Character>())
                {
                    var stormHitCharacter = stormHitRoot.GetComponent<Character>();
                    Debug.Log($"Player Inside SafeZone: {stormHitCharacter.Id}, size of Sphere Radius: {transform.localScale.x}");
                    m_playersInZone.Add(stormHitCharacter.Player, stormHitCharacter);
                }
            }
        }
        damagePlayersCallback();
    }

    private void ShrinkToDataPoint(ref StormData data)
    {
        //if (m_stormShrinkWaitTimer.ExpiredOrNotRunning(Runner)) m_stormShrinkWaitTimer = TickTimer.CreateFromTicks(Runner, m_ticksPerSecond);
        //if (m_stormShrinkWaitTimer.RemainingTicks(Runner) != m_ticksPerSecond) return;
        data.ElapsedTime += Runner.DeltaTime;
        GameLogicManager.Instance.StartStormPhase(Mathf.CeilToInt(data.WaitTime), 9999);
        if (data.ElapsedTime < data.WaitTime)
        {
            return;
        }
        GameLogicManager.Instance.StartStormPhase(Mathf.CeilToInt(data.ShrinkTime), 9999);

        data.ShrinkTime += Runner.DeltaTime;

        var curProg = data.ShrinkTime / data.TimeToClose;

        transform.position = Vector3.Lerp(transform.position, data.EndPos, curProg);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(data.Size, data.Size, data.Size), curProg);

        if (curProg >= 1)
        {
            CurrentStormPhase++;
        }
    }

}
